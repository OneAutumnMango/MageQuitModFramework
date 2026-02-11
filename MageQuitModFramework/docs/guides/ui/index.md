# UI - User Interface System

The UI namespace provides a dynamic mod menu and component registry.

## DynamicModMenu

In-game menu for mod configuration, toggled with **F5**.

### Features

- Automatically displays all registered mods
- Expandable/collapsible mod panels
- Custom UI per mod
- Always accessible during gameplay

### Opening the Menu

Press **F5** during gameplay to toggle the mod menu.

## ModUIRegistry

Registry for mod UI components.

### Registering Your Mod UI

```csharp
using MageQuitModFramework.UI;

ModUIRegistry.RegisterMod(
    "MyMod",
    () => DrawMyUI()
);

private void DrawMyUI()
{
    GUILayout.Label("MyMod Settings");
    
    if (GUILayout.Button("Toggle Feature"))
    {
        myFeatureEnabled = !myFeatureEnabled;
    }
    
    GUILayout.Label($"Status: {(myFeatureEnabled ? "On" : "Off")}");
}
```

### Unregistering

```csharp
protected override void OnUnload(Harmony harmony)
{
    ModUIRegistry.UnregisterMod("MyMod");
}
```

## StyleManager

Provides pre-configured GUIStyle objects.

### Available Styles

```csharp
using MageQuitModFramework.UI;

// In OnGUI
if (GUILayout.Button("Positive Action", StyleManager.Green))
{
    // Green button
}

if (GUILayout.Button("Negative Action", StyleManager.Red))
{
    // Red button
}

GUILayout.Label("Common Item", StyleManager.CommonStyle);    // White
GUILayout.Label("Rare Item", StyleManager.RareStyle);        // Purple
GUILayout.Label("Legendary Item", StyleManager.LegendaryStyle); // Gold
```

## UIComponents

Utility functions for common UI patterns.

### Creating Labels

```csharp
using MageQuitModFramework.UI;
using UnityEngine;

GUILayout.Label("Simple label");
GUILayout.Label("Colored label", new GUIStyle { normal = { textColor = Color.cyan } });
```

### Creating Buttons

```csharp
if (GUILayout.Button("Click Me"))
{
    // Button clicked
}

if (GUILayout.Button("Styled Button", StyleManager.Green))
{
    // Green button clicked
}
```

### Layout Helpers

```csharp
// Horizontal layout
GUILayout.BeginHorizontal();
GUILayout.Label("Label:");
if (GUILayout.Button("Button")) { }
GUILayout.EndHorizontal();

// Vertical layout (default)
GUILayout.BeginVertical();
GUILayout.Label("First");
GUILayout.Label("Second");
GUILayout.EndVertical();

// Scrollable area
scrollPosition = GUILayout.BeginScrollView(scrollPosition);
// ... content ...
GUILayout.EndScrollView();
```

## Complete Example

```csharp
using BepInEx;
using MageQuitModFramework.UI;
using MageQuitModFramework.Modding;
using UnityEngine;

[BepInPlugin("com.example.mymod", "MyMod", "1.0.0")]
[BepInDependency("com.magequit.modframework")]
public class MyModPlugin : BaseUnityPlugin
{
    private bool featureEnabled = false;
    private float multiplier = 1.5f;
    private Vector2 scrollPos;

    private void Awake()
    {
        ModUIRegistry.RegisterMod("MyMod", DrawUI);
    }

    private void DrawUI()
    {
        GUILayout.Label("=== MyMod Settings ===");
        
        // Toggle feature
        if (GUILayout.Button(
            featureEnabled ? "Disable Feature" : "Enable Feature",
            featureEnabled ? StyleManager.Red : StyleManager.Green))
        {
            featureEnabled = !featureEnabled;
        }
        
        // Slider
        GUILayout.Label($"Multiplier: {multiplier:F2}x");
        multiplier = GUILayout.HorizontalSlider(multiplier, 0.5f, 3.0f);
        
        // Reset button
        if (GUILayout.Button("Reset to Default"))
        {
            multiplier = 1.5f;
            featureEnabled = false;
        }
        
        // Scrollable log
        GUILayout.Label("Recent Actions:");
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(100));
        GUILayout.Label("Action 1");
        GUILayout.Label("Action 2");
        GUILayout.Label("Action 3");
        GUILayout.EndScrollView();
    }

    private void OnDestroy()
    {
        ModUIRegistry.UnregisterMod("MyMod");
    }
}
```

## Best Practices

### Keep UI Simple

The mod menu is for configuration, not gameplay HUD. Keep it focused on settings.

```csharp
// Good
GUILayout.Label("Damage Multiplier: 1.5x");
if (GUILayout.Button("Reset")) { }

// Avoid complex layouts
// Don't create entire game UIs here
```

### Cache Styles

Don't create new GUIStyle objects every frame:

```csharp
// Bad - creates new style every frame
var style = new GUIStyle { normal = { textColor = Color.red } };
GUILayout.Label("Text", style);

// Good - use StyleManager or cache
GUILayout.Label("Text", StyleManager.Red);
```

### Use Descriptive Labels

```csharp
// Bad
GUILayout.Label("1.5x");

// Good
GUILayout.Label("Damage Multiplier: 1.5x");
```

### Group Related Settings

```csharp
GUILayout.Label("=== Combat Settings ===");
// ... combat options ...

GUILayout.Space(10);

GUILayout.Label("=== Visual Settings ===");
// ... visual options ...
```
