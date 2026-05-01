# SN41 Discord Bot — Feature Task List

A language-agnostic specification of every feature the bot implements.
Checked items = done. Unchecked items = to implement.

---

## Environment & Configuration

- [x] Read the following values from environment variables at startup:
  - `DISCORD_BOT_SECRET_KEY` — bot authentication token
  - `DISCORD_DUPLICATE_VOICE_CHANNEL` — ID of the voice channel that triggers cloning
  - `DISCORD_GENERAL_TEXT_CHANNEL` — ID of the text channel for goodbye messages
  - `DISCORD_ASSIGNROLE_TEXT_CHANNEL` — ID of the text channel for role assignment UI and logs
  - `DISCORD_ADMIN_LOG_CHANNEL` — ID of the text channel for security/admin audit logs
  - `RENDER_API_KEY` — API key for the replay rendering service

---

## Admin Roles

- [x] Treat the following role names as admin roles (hardcoded):
  `ADEPTUS MECHANICUS`, `AMMIRAGLIO`, `RECLUTATORE [SN41]`, `COMMODORO`, `BOT`
- [x] Any check for "is this user an admin?" means: does the user hold at least one of
  those roles.

---

# Features

- [x] Feature 1 — Welcome New Members
- [x] Feature 2 — Goodbye Messages
- [ ] Feature 3 — Role Assignment UI
- [ ] Feature 4 — Temporary Voice Channels
- [x] Feature 5 — Anti-Spam
- [x] Feature 6 — Replay Rendering (`/replay`)

---

## Feature 1 — Welcome New Members

**Trigger:** A user joins the guild.

- [x] Send the new member a **direct message** containing:
  - A welcome image (`discord_msg_img.png`)
  - The following text (Italian, keep as-is):
    ```
    Benvenuto in **SN41 COMMUNITY** <member name>!
    Modifica il tuo soprannome nel nostro discord in modo che coincida con il tuo nickname
    in gioco e se vuoi metti il tuo nome tra parentesi.
    Se sei interessato ad entrare nel clan, non esitare a contattare uno degli admin.
    Buona permanenza !
    ```
  - If the DM cannot be delivered (user has DMs disabled), log the failure and continue.
- [ ] Post a **role-selection UI** (see Feature 3) in `DISCORD_ASSIGNROLE_TEXT_CHANNEL`
  targeting the new member.

---

## Feature 2 — Goodbye Messages

**Trigger:** A user leaves the guild.

- [x] Pick a random line from `goodbye_phrases.txt`.
- [x] Replace the `{}` placeholder in that line with the member's name.
- [x] Post the result in `DISCORD_GENERAL_TEXT_CHANNEL`.

### goodbye_phrases.txt

One phrase per line. Each line must contain `{}` as the member name placeholder.
Use ~21 Italian, naval-themed farewell phrases. Examples:
```
{} è salpato verso lidi lontani.
{} ha lasciato il convoglio. Rimanete allerta, equipaggio.
Sottomarino U-Boat in ritirata, {} ha lasciato la formazione.
```

---

## Feature 3 — Role Assignment UI

Members are assigned to clans via a button-based UI. Roles are resolved dynamically from
the guild's role list, not hardcoded.

### Discovering clan roles

- [ ] Read the guild's role list and locate two marker roles: `SN41 COMMUNITY` and `COMMODORO`.
- [ ] All roles positioned **between** these two markers (exclusive) are treated as clan roles.
- [ ] The ordered list of selectable options is: `[SN41]` first, then the clan roles in
  **reverse** discovery order (newest clan last in the list = first displayed after `[SN41]`),
  then `OSPITI` last.
- [ ] Each option maps to a set of roles to assign:
  - `[SN41]` → assign roles `[SN41]` and `SN41 COMMUNITY`
  - Any other clan → assign that clan's role and `SN41 COMMUNITY`
  - `OSPITI` → assign only `OSPITI`

### Showing the UI

- [ ] Display one button per selectable option.
- [ ] When invoked from the `/cambiaruolo` slash command, the UI is **ephemeral** (visible
  only to the invoking admin).
- [ ] When shown automatically on member join, the UI is **public** in the channel.

### Clicking a button

- [ ] Verify the **clicking user** is an admin. If not: log the attempt as a security
  warning to `DISCORD_ADMIN_LOG_CHANNEL` and take no action.
- [ ] Verify the **target member** is not an admin. If they are: log the attempt as a
  security warning to `DISCORD_ADMIN_LOG_CHANNEL` and take no action.
- [ ] Remove **all** existing roles from the target member.
- [ ] Assign the roles corresponding to the clicked option.
- [ ] Remove the button UI from the channel (delete or edit the message).
- [ ] Post a log line to `DISCORD_ASSIGNROLE_TEXT_CHANNEL`:
  `<admin name> ha cambiato il ruolo di <member name> a <clan name>`

### `/cambiaruolo` command

- [ ] Admin-only slash command (reject non-admins silently or with an error).
- [ ] Parameter: a guild member.
- [ ] Must be used inside `DISCORD_ASSIGNROLE_TEXT_CHANNEL`; reject if used elsewhere.
- [ ] Posts the role-selection UI (ephemeral) targeting the specified member.

---

## Feature 4 — Temporary Voice Channels

One specific voice channel (`DISCORD_DUPLICATE_VOICE_CHANNEL`) acts as a permanent
"lobby". Joining it creates a private numbered clone; the clone is deleted when it empties.

### Creating a clone

**Trigger:** A user joins `DISCORD_DUPLICATE_VOICE_CHANNEL`.

- [ ] Enforce a cap of **50** simultaneous clones. If the cap is reached:
  - Disconnect the user immediately.
  - Send them a DM: `"Vile marrano! Limite stanze a 50! 🗿🗿🗿"`
  - Do not create a channel.
- [ ] Assign the clone a number using **gap-filling**: find the lowest positive integer not
  already in use by an existing clone (e.g. if 1, 2, 4 exist → assign 3).
- [ ] Create a voice channel with:
  - Name: `<number> <template channel name>`
  - Same category as the template channel
  - Same permission overwrites as the template channel
  - Position: template channel position + assigned number
- [ ] Move the user into the newly created channel.

### Deleting a clone

**Trigger:** A user leaves a cloned channel and the channel is now empty.

- [ ] Delete the channel from the guild.
- [ ] Free its number so it can be reused.

### Edge cases

- [ ] Ignore voice state events that are not actual channel changes (e.g. mute/unmute,
  camera on/off) — these fire the same event but the user's channel has not changed.

---

## Feature 5 — Anti-Spam

**Trigger:** Any message posted in the guild.

- [x] Ignore messages from bots.
- [x] Ignore messages from admins.
- [x] Ignore messages shorter than **10 characters**.
- [x] For every other message, track per-user state:
  - The hash of their last seen message content (use MD5 or equivalent)
  - The timestamp of the first occurrence
  - How many times it has been repeated
  - References to the messages themselves (for deletion)
- [x] **Time window:** if the same user sends a message more than **1 minute** after their
  last tracked message, reset the tracking state.
- [x] **Different message:** if the user's new message has a different hash than the tracked
  one, reset the state.
- [x] **Same message repeated (2nd occurrence):** begin accumulating — no action yet.
- [x] **Same message repeated (3rd occurrence):**
  - Delete all accumulated copies (including the current one).
  - Send the user a DM (Italian, keep as-is):
    ```
    Rilevati messaggi ripetuti.
    Messaggi precedenti cancellati.
    Ulteriori invii dello stesso messaggio di seguito risulteranno in un timeout di 5 minuti.

    In caso di domande o falsi positivi, contattare @eisterman
    ```
  - If the DM cannot be delivered, log the failure and continue.
- [x] **Same message repeated (4th occurrence and beyond):**
  - Apply a **5-minute timeout** to the user (reason: `"Spamming"`).
  - Notify `DISCORD_ADMIN_LOG_CHANNEL`.

---

## Feature 6 — Replay Rendering (`/replay`)

- [x] Global slash command `/replay` with one required parameter: a file attachment.
- [x] Reject attachments whose filename does not end in `.wowsreplay`.
- [x] Show a loading/thinking indicator while processing (rendering can take several minutes).
- [x] Upload the file to the render API:
  - `POST https://renderapi.sn41.eisterman.dev/api/render`
  - Header: `X-API-KEY: <RENDER_API_KEY>`
  - Body: multipart form with the file
- [x] The API responds with JSON:
  ```json
  { "video_url": "/api/render/output/<uuid>.mp4", "metadata": { "filename": "…" } }
  ```
- [x] Download the video from `https://renderapi.sn41.eisterman.dev<video_url>`.
- [x] Send the video file back to the user in Discord, together with an embed showing the
  original filename from `metadata.filename`.
- [x] On any error, respond with: `"ERRORE! Urla a Fede di tornare in miniera"`

---

## Deployment

- [ ] Bot runs as a single long-lived process (no scheduled jobs, purely event-driven).
- [ ] All state (spam tracking, active voice clones) is in-memory and resets on restart.
- [ ] Containerise with Docker; load environment from `.env`; restart automatically on crash.
