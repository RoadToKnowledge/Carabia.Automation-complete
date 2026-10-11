# Saved sessions (git-ignored)

After you log in manually, the framework stores the browser state of that role here:

```
sessions/buyer/session.json
sessions/seller/session.json
sessions/administrator/session.json
```

Each file holds only cookies (including `Carabia.Auth`), localStorage/sessionStorage of the Carabia site,
the base URL and the capture time. No phone number, password or OTP is ever written.

- A session is reused while the server still accepts it (checked with `/Identity/AuthStatus`).
- It is ignored when it belongs to another base URL, is older than `Authentication.SessionMaxAgeHours`,
  or its `Carabia.Auth` cookie has expired - then you are asked to log in again.
- Delete a file (or run `scripts/clean-results.ps1 -Sessions`) to force a new login.

Everything in this folder except this README and `.gitkeep` is ignored by git. Never commit session files:
they are equivalent to being logged in as that account.
