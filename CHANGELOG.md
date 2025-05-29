# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## Added

- Added arguments guards to the language, they can be used to prevent script execution when there are not enough arguments provided.

## Changed

- File scripts loader events handler now provides more usefull arguments.
- Changed the consoles scope name from `GameConsole` to `Client`.
- File scripts loader components now utilize dispose pattern properly.
- File scripts loader was updated to no longer rely on static properties and files/directories location tracking.
- Upgraded project to [LabAPI v1.0.2](https://github.com/northwood-studios/LabAPI/releases/tag/1.0.2).

## [1.0.2]- 2024-11-21

### Fixed

- Fixed invalid or missing in-code documentation.

## [1.0.1] - 2024-10-18

### Fixed

- Fixed in-directive comments messing up line extensions.

## [1.0.0] - 2024-10-17

### Added

- Initial plugin version made with [NwPluginAPI v13.1.2](https://github.com/northwood-studios/NwPluginAPI/releases/tag/13.1.2).
