# Upload fixtures

- `photos/` - five synthetic JPEG images (generated, no real vehicle or person) used by the Sell My Car photo step.
  The wizard needs at least 4 photos. Override the folder with the `CARABIA_SELL_PHOTO_DIR` environment variable.
- `invalid-file.txt` - a non-image file used by the invalid-upload test.

Photos stay local browser previews until a listing is submitted. Submission only happens in the guarded
`SubmissionTests` (Execution.AllowSellSubmissionTests=true on DEV).
