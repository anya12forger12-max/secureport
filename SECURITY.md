# Security Policy

## Supported Versions

| Version | Supported          |
|---------|--------------------|
| 1.0.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

If you discover a security vulnerability in SecurePort, please report it responsibly.

**Do not open a public GitHub issue for security vulnerabilities.**

Instead, email **secureport-security@proton.me** with:

- A description of the vulnerability
- Steps to reproduce
- Potential impact assessment
- Any suggested fixes (optional)

### What Counts as a Security Issue

- Remote code execution or arbitrary file access
- Authentication or authorization bypass
- Path traversal allowing access outside the data directory
- Injection vulnerabilities (command, path, or data)
- Cryptographic weaknesses in AES-256-GCM implementation
- Insecure data storage or leakage of scan results
- Deserialization vulnerabilities in JSON handling
- Denial of service through malformed input

### Response Timeline

| Action | Timeframe |
|--------|-----------|
| Acknowledgement | Within 48 hours |
| Initial assessment | Within 5 business days |
| Fix or mitigation | Within 30 days for critical, 90 days for moderate |
| Public disclosure | After fix is released |

### Scope

This policy applies to the current release of SecurePort as hosted in this repository.

**In scope:**
- The SecurePort application code
- Storage and encryption implementations
- Import/export parsing logic
- UI input validation

**Out of scope:**
- Third-party dependencies (report to upstream)
- Social engineering attacks
- Physical attacks on infrastructure
- Issues requiring modification of the operating system

## Safe Harbor

We support responsible disclosure and will not pursue legal action against researchers who:

- Make a good faith effort to avoid privacy violations and data destruction
- Only interact with accounts you own or with explicit permission of the account holder
- Do not exploit a vulnerability beyond what is necessary to confirm its existence
- Report vulnerabilities promptly and do not publicly disclose before a fix is available

## Preferred Languages

We accept vulnerability reports in English.
