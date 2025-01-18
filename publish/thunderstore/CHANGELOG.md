# Changelog

All notable changes will be documented in this file.
This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 1.0.1
- Fix `GetTootipData` patch exception when null tooltips are set on the Item, and no localised strings were provided.
  - This is unity's fault. Tooltips should be serialised correctly on Item resources, but they aren't loaded, so the count is correct but the text is null.
- Added `SetDefaultTooltips` extension method to set fallback tooltips.

## 1.0.0
- Initial Release