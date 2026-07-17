using System.Collections.Immutable;

namespace SecurePort.Accessibility.Services;

/// <summary>
/// Defines the priority level for a screen reader announcement.
/// </summary>
public enum AnnouncementPriority
{
    /// <summary>
    /// A polite announcement that waits for the screen reader to finish its current utterance.
    /// </summary>
    Polite,

    /// <summary>
    /// An assertive announcement that interrupts the screen reader immediately.
    /// </summary>
    Assertive
}

/// <summary>
/// Represents a tracked focus element within the application.
/// </summary>
/// <param name="ElementId">A unique identifier for the focusable element.</param>
/// <param name="Label">The accessible label describing the element.</param>
/// <param name="IsEnabled">Whether the element is currently interactable.</param>
public sealed record FocusElement(string ElementId, string Label, bool IsEnabled = true);

/// <summary>
/// Manages accessibility features including screen reader announcements,
/// keyboard navigation state, and focus management.
/// </summary>
public sealed class AccessibilityService
{
    private readonly List<FocusElement> _focusOrder = new();
    private readonly Stack<string> _focusHistory = new();
    private int _focusIndex = -1;
    private bool _keyboardNavigationActive;

    /// <summary>
    /// Raised when a screen reader announcement should be displayed.
    /// </summary>
    public event EventHandler<AccessibilityAnnouncementEventArgs>? AnnouncementRequested;

    /// <summary>
    /// Raised when keyboard navigation mode is toggled.
    /// </summary>
    public event EventHandler<bool>? KeyboardNavigationChanged;

    /// <summary>
    /// Raised when the focused element changes.
    /// </summary>
    public event EventHandler<FocusElement>? FocusChanged;

    /// <summary>
    /// Gets a value indicating whether keyboard navigation mode is currently active.
    /// Keyboard navigation is activated when the user presses Tab and deactivated on mouse click.
    /// </summary>
    public bool IsKeyboardNavigationActive => _keyboardNavigationActive;

    /// <summary>
    /// Gets the currently focused element, or <c>null</c> if nothing is focused.
    /// </summary>
    public FocusElement? CurrentFocus => _focusIndex >= 0 && _focusIndex < _focusOrder.Count
        ? _focusOrder[_focusIndex]
        : null;

    /// <summary>
    /// Gets the immutable list of registered focusable elements in tab order.
    /// </summary>
    public IReadOnlyList<FocusElement> FocusOrder => _focusOrder.ToImmutableList();

    /// <summary>
    /// Registers a focusable element for keyboard navigation tracking.
    /// </summary>
    /// <param name="element">The element to register.</param>
    public void RegisterFocusElement(FocusElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        _focusOrder.RemoveAll(e => e.ElementId == element.ElementId);
        _focusOrder.Add(element);
    }

    /// <summary>
    /// Removes a previously registered focusable element.
    /// </summary>
    /// <param name="elementId">The unique identifier of the element to remove.</param>
    /// <returns><c>true</c> if the element was found and removed; otherwise, <c>false</c>.</returns>
    public bool UnregisterFocusElement(string elementId)
    {
        var removed = _focusOrder.RemoveAll(e => e.ElementId == elementId) > 0;

        if (_focusIndex >= _focusOrder.Count)
            _focusIndex = _focusOrder.Count - 1;

        return removed;
    }

    /// <summary>
    /// Moves keyboard focus to the next element in the tab order.
    /// Wraps to the first element when the end is reached.
    /// </summary>
    /// <returns>The newly focused element, or <c>null</c> if no elements are registered.</returns>
    public FocusElement? MoveFocusNext()
    {
        if (_focusOrder.Count == 0)
            return null;

        // Push current position onto history stack for back-navigation.
        if (_focusIndex >= 0)
            _focusHistory.Push(_focusIndex.ToString());

        _focusIndex = (_focusIndex + 1) % _focusOrder.Count;
        var element = _focusOrder[_focusIndex];

        FocusChanged?.Invoke(this, element);
        return element;
    }

    /// <summary>
    /// Moves keyboard focus to the previous element in the tab order.
    /// Wraps to the last element when the beginning is reached.
    /// </summary>
    /// <returns>The newly focused element, or <c>null</c> if no elements are registered.</returns>
    public FocusElement? MoveFocusPrevious()
    {
        if (_focusOrder.Count == 0)
            return null;

        _focusIndex = (_focusIndex - 1 + _focusOrder.Count) % _focusOrder.Count;
        var element = _focusOrder[_focusIndex];

        FocusChanged?.Invoke(this, element);
        return element;
    }

    /// <summary>
    /// Moves keyboard focus to a specific element by its identifier.
    /// </summary>
    /// <param name="elementId">The unique identifier of the target element.</param>
    /// <returns>The focused element if found; otherwise, <c>null</c>.</returns>
    public FocusElement? MoveFocusTo(string elementId)
    {
        var index = _focusOrder.FindIndex(e => e.ElementId == elementId);
        if (index < 0)
            return null;

        if (_focusIndex >= 0)
            _focusHistory.Push(_focusIndex.ToString());

        _focusIndex = index;
        var element = _focusOrder[_focusIndex];

        FocusChanged?.Invoke(this, element);
        return element;
    }

    /// <summary>
    /// Clears all focus and resets navigation state.
    /// </summary>
    public void ClearFocus()
    {
        _focusIndex = -1;
        _focusHistory.Clear();
    }

    /// <summary>
    /// Activates keyboard navigation mode. This should be called when a Tab key
    /// press is detected, suppressing mouse-driven focus behavior.
    /// </summary>
    public void ActivateKeyboardNavigation()
    {
        if (_keyboardNavigationActive)
            return;

        _keyboardNavigationActive = true;
        KeyboardNavigationChanged?.Invoke(this, true);
    }

    /// <summary>
    /// Deactivates keyboard navigation mode. This should be called when a mouse
    /// click or touch event is detected, restoring normal focus behavior.
    /// </summary>
    public void DeactivateKeyboardNavigation()
    {
        if (!_keyboardNavigationActive)
            return;

        _keyboardNavigationActive = false;
        KeyboardNavigationChanged?.Invoke(this, false);
    }

    /// <summary>
    /// Requests a polite screen reader announcement. The announcement will be
    /// queued and spoken after the current utterance finishes.
    /// </summary>
    /// <param name="message">The text to announce.</param>
    public void AnnouncePolite(string message)
    {
        RaiseAnnouncement(message, AnnouncementPriority.Polite);
    }

    /// <summary>
    /// Requests an assertive screen reader announcement. The announcement will
    /// interrupt any current speech immediately.
    /// </summary>
    /// <param name="message">The text to announce.</param>
    public void AnnounceAssertive(string message)
    {
        RaiseAnnouncement(message, AnnouncementPriority.Assertive);
    }

    /// <summary>
    /// Requests a screen reader announcement at the specified priority level.
    /// </summary>
    /// <param name="message">The text to announce.</param>
    /// <param name="priority">The priority controlling when the announcement is spoken.</param>
    public void Announce(string message, AnnouncementPriority priority = AnnouncementPriority.Polite)
    {
        RaiseAnnouncement(message, priority);
    }

    /// <summary>
    /// Checks whether the given key event represents a Tab navigation key.
    /// </summary>
    /// <param name="key">The key identifier to check.</param>
    /// <param name="modifiers">Any modifier keys held during the press.</param>
    /// <returns><c>true</c> if the combination represents forward or backward tab navigation.</returns>
    public static bool IsTabNavigation(string key, string modifiers = "")
    {
        var isShift = modifiers.Contains("Shift", StringComparison.OrdinalIgnoreCase);
        return key.Equals("Tab", StringComparison.OrdinalIgnoreCase) ||
               (isShift && key.Equals("Tab", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the number of registered focusable elements.
    /// </summary>
    public int FocusElementCount => _focusOrder.Count;

    /// <summary>
    /// Raises the <see cref="AnnouncementRequested"/> event.
    /// </summary>
    private void RaiseAnnouncement(string message, AnnouncementPriority priority)
    {
        AnnouncementRequested?.Invoke(this, new AccessibilityAnnouncementEventArgs(message, priority));
    }
}

/// <summary>
/// Event arguments for a screen reader announcement request.
/// </summary>
/// <param name="Message">The text content to announce.</param>
/// <param name="Priority">The priority controlling when the announcement is spoken.</param>
public sealed record AccessibilityAnnouncementEventArgs(
    string Message,
    AnnouncementPriority Priority);
