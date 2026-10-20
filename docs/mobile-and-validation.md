# Mobile policy and validation

MOBILE-01: The frame-rate service accepts 30 as the battery-saving target and normalizes all other requests to 60. It disables vSync and applies the requested target through Unity. These are policy targets, not measured device performance.

VERIFY-01: The inspected EditMode test source declares nine test methods and twelve cases: eight individual tests and four parameterized frame-rate cases. It covers defaults, frame-rate normalization, settings bounds, save serialization and paths, scene-name validation, and safe-area math. Discovery and execution have not been verified in this review.

Unity import, compilation, Play Mode, Android builds, touch input, lifecycle behavior, physical-device performance, and store readiness remain unverified here. Documentation checks cannot establish those outcomes. Before claiming them, record the exact source revision, environment, test procedure, results, and any retained artifacts.
