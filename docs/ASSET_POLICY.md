# Assets, dependencies and redistribution

The source cleanup for GitHub keeps code and required Unity source structure while separating large, potentially unlicensed or reproducible files.

## Included in source

- Unity scripts, packages/configuration, scenes, chapter JSON data, generated model assets and their `.meta` GUID files.
- Story, approved dialogue, character-design references, small source art and technical documentation.
- Domain/dialogue tests and reproducible generation scripts.

## Excluded from Git (retained locally when useful)

| Path | Reason |
| --- | --- |
| `AUDIO/` | Source sound packages; redistribution rights not verified |
| `Game/Assets/_Civi/Resources/Audio/` | Generated Ogg imports derived from the sound packs |
| `source-assets/audio/` | Duplicate of the Chapter 1 package, removed from the cleaned local source |
| `release/` | Generated Windows player/build output |
| `Game/Library/`, `Game/Temp/`, `Game/Logs/` | Unity-created cache and logs |
| `artifacts/` | Screenshots, QA dumps, rejected experiments |
| `tools/vendor/` | Unreferenced assets from abandoned semi-realistic character experiments |

The raw sound packages have **not** been deleted from `AUDIO/` on the working machine. Removal of a duplicate does not grant permission to redistribute the surviving files.

## Release checklist

1. Confirm ownership and distribution licenses for soundtrack/effects, fonts, third-party assets, and character references.
2. Provide license texts and attribution in `licenses/` as required.
3. Check that generated audio and unapproved media are not accidentally added to Git or a public release archive.
4. Run source tests, Unity build, gameplay QA, and manual audio/visual review.
5. Publish binaries or enable Git LFS **only after** verifying rights and storage policy.

No general permission to reuse or redistribute the project's assets is implied by access to this repository.
