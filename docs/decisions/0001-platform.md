# ADR 0001 — Windows WPF and isolated core

2026-10-16. Choose C#/.NET 10 LTS (SDK 10.0.402 installed locally) with WPF Viewport3D for the initial Windows-first application. Microsoft's documented MeshGeometry3D, perspective camera, normals/UV/materials and depth-tested rendering provide real 3D without implementing an OpenGL host before native integration. This is the evidence-based deviation from the default Avalonia suggestion. CPU skinning is shared/testable; WPF owns only rendering/UI. No painter sorting. GPU skinning/large-model optimization and cross-platform Avalonia UI remain possible, not delivered.

Core .NET 10; desktop net10.0-windows. Original installed SDK 9.0.300 is not used. Local installer changes no global PATH. Desktop CLR and BrawlLib legacy CLR stay separate. No AI/DCC runtime required. WPF UI uses imperative controls and a headless EditSession; complete MVVM refactoring is deferred.

References checked: https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/3-d-graphics-overview ; https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core ; official 10.0 release metadata used by setup.
