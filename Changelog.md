# Changelog

## v1.0.1 - 20 February 2026
- Fixed a bug which caused custom events to sometimes be ignored if another custom event existed with the same type and beat.

## v1.0.0 - 19 February 2026
- Replaced the version check with an automatic version control system which retrieves up-to-date information about mod compatibility. This means that new releases no longer have to be created when the game updates if there are no breaking changes.
- Provided a library which provides various useful utilities for other mods, including:
  - Automatic integration with the new version control system.
  - Customization of the mod's appearance in the settings menu (currently only supports overriding the inferred spacing in the mod's name).
  - Utilities for creating config options, logging, and retrieving plugin metadata.
  - A system for handling custom beatmap events and preventing conflicts with other mods.
- Configuration options of string type now render in the settings menu as text input fields.

## v0.2.9 - 10 December 2025
Updated game version. Compatible with Patch 1.11.1.

## v0.2.8 - 30 October 2025
Updated game version. Compatible with Patch 1.10.0.

## v0.2.7 - 18 September 2025
Updated game version. Compatible with Patch 1.8.0.

## v0.2.6 - 15 September 2025
Updated game version. Compatible with Patch 1.7.1.

## v0.2.5 - 14 August 2025
Updated game version. Compatible with Patch 1.7.0.

## v0.2.4 - 25 June 2025
Updated game version. Compatible with Patch 1.6.0.

## v0.2.3 - 21 June 2025
Updated game version. Compatible with Patch 1.5.1.

## v0.2.2 - 14 June 2025
- Relaxed the version check to ignore the specific build number in order to reduce superfluous updates.

Updated game version. Compatible with Patch 1.5.0.

## v0.2.1 - 12 June 2025
- Added a manual override to the version control check. If you know a game update won't break the mod, you can set the [Version Control].[Version Override] option to the current build number to skip the check.

Updated game version. Compatible with Patch 1.5.0-b20869.

## v0.2.0 - 12 June 2025
Updated game version. Compatible with Patch 1.5.0-b20860.

## v0.1.0 - 11 May 2025
Initial release. Compatible with Patch 1.4.0-b20638.
