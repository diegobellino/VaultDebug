# Changelog

All notable changes to VaultDebug are documented in this file.

## [0.0.3] - 2026-07-19

### Added

- Added an optional color parameter to `VaultLogger`.
- Added a Unity Console log handler as the default output.
- Added dedicated `VaultLogger` usage documentation.

### Changed

- Reworked the expanded log view.
- Added .NET 10 and Newtonsoft.Json dependencies.

## [0.0.2] - 2025-02-24

### Added

- Added a benchmark option to the logger sample.
- Documented known issues.

### Changed

- Moved the package code into the Unity package directory and removed the Unity project from the repository.
- Updated package metadata and documentation.

### Fixed

- Correctly reuse logs through the `VaultPool` system.
- Enforced a minimum height for the console detail view.
- Resolved visual issues.
- Fixed compilation, package metadata, and package structure issues.

## [0.0.1] - 2025-02-23

### Added

- Initial Vault Console and `VaultLogger` implementations.
- Editor Console log details, focused-log persistence, hover styling, and automatic scrolling to selected and newest logs.
- Smart and raw stack trace tabs.
- Main-thread log dispatching and performance improvements.
- Tag filtering, persisted log storage, sorted logs, and optimized context-based lookup.
- Dependency injection through `DIBootstrapper`, `DIContainer`, `LoggerProvider`, and DI-friendly log storage and pooling.
- Structured logging support, sample loggers, XML documentation, and a project README.
- Unit tests and a CI workflow that runs tests on pushes and manual dispatch.

### Changed

- Updated the project to Unity 2021.3.14.
- Improved toolbar organization, timestamp resolution, and the project and package structure.

### Fixed

- Corrected visual issues, log clearing, compilation paths, test structure, and test-runner configuration.
