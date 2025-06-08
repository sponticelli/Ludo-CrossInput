using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

namespace Ludo.CrossInput
{
    /// <summary>
    /// Setup wizard to help configure InputManager with existing Input Actions files.
    /// </summary>
    public class InputManagerSetupWizard : EditorWindow
    {
        private InputActionAsset sourceInputActions;
        private InputActionMapping targetMapping;
        private Vector2 scrollPosition;
        private bool showAdvancedOptions = false;
        
        private readonly Dictionary<string, string> suggestedMappings = new Dictionary<string, string>();
        private readonly Dictionary<string, string> customMappings = new Dictionary<string, string>();

        [MenuItem("Tools/Ludo/CrossInput/Setup Wizard")]
        public static void ShowWindow()
        {
            var window = GetWindow<InputManagerSetupWizard>("InputManager Setup");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("InputManager Setup Wizard", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawInputActionsSelection();
            EditorGUILayout.Space();

            if (sourceInputActions != null)
            {
                DrawActionMappingSection();
                EditorGUILayout.Space();
                DrawActionButtons();
            }
        }

        private void DrawInputActionsSelection()
        {
            EditorGUILayout.LabelField("Step 1: Select Your Input Actions", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Select your existing Input Actions asset to automatically generate mappings.", MessageType.Info);
            
            var newInputActions = (InputActionAsset)EditorGUILayout.ObjectField(
                "Input Actions Asset", 
                sourceInputActions, 
                typeof(InputActionAsset), 
                false
            );

            if (newInputActions != sourceInputActions)
            {
                sourceInputActions = newInputActions;
                if (sourceInputActions != null)
                {
                    AnalyzeInputActions();
                }
            }
        }

        private void DrawActionMappingSection()
        {
            EditorGUILayout.LabelField("Step 2: Configure Action Mappings", EditorStyles.boldLabel);
            
            targetMapping = (InputActionMapping)EditorGUILayout.ObjectField(
                "Target Mapping Asset", 
                targetMapping, 
                typeof(InputActionMapping), 
                false
            );

            if (GUILayout.Button("Create New Mapping Asset"))
            {
                CreateNewMappingAsset();
            }

            EditorGUILayout.Space();

            showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Action Mappings");
            
            if (showAdvancedOptions)
            {
                DrawActionMappings();
            }
        }

        private void DrawActionMappings()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            
            EditorGUILayout.LabelField("Expected Action → Your Action", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            var expectedActions = new[]
            {
                InputActionNames.MOVE, InputActionNames.FIRE, InputActionNames.JUMP,
                InputActionNames.CROUCH, InputActionNames.SPRINT, InputActionNames.RELOAD,
                InputActionNames.INVENTORY, InputActionNames.INTERACT, InputActionNames.MAP,
                InputActionNames.PREVIOUS, InputActionNames.NEXT, InputActionNames.PAUSE,
                InputActionNames.BACK, InputActionNames.LEFT, InputActionNames.RIGHT,
                InputActionNames.PRESS, InputActionNames.POSITION
            };

            foreach (var expectedAction in expectedActions)
            {
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField(expectedAction, GUILayout.Width(100));
                EditorGUILayout.LabelField("→", GUILayout.Width(20));
                
                string currentMapping = "";
                if (customMappings.ContainsKey(expectedAction))
                {
                    currentMapping = customMappings[expectedAction];
                }
                else if (suggestedMappings.ContainsKey(expectedAction))
                {
                    currentMapping = suggestedMappings[expectedAction];
                }

                var newMapping = EditorGUILayout.TextField(currentMapping);
                
                if (newMapping != currentMapping)
                {
                    customMappings[expectedAction] = newMapping;
                }

                // Show suggestion if available
                if (suggestedMappings.ContainsKey(expectedAction) && 
                    suggestedMappings[expectedAction] != newMapping)
                {
                    if (GUILayout.Button($"Use '{suggestedMappings[expectedAction]}'", GUILayout.Width(120)))
                    {
                        customMappings[expectedAction] = suggestedMappings[expectedAction];
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.LabelField("Step 3: Generate Configuration", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Auto-Map Actions"))
            {
                AutoMapActions();
            }
            
            if (GUILayout.Button("Apply Mappings") && targetMapping != null)
            {
                ApplyMappings();
            }
            
            if (GUILayout.Button("Validate Mappings") && targetMapping != null)
            {
                ValidateMappings();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void AnalyzeInputActions()
        {
            suggestedMappings.Clear();
            
            if (sourceInputActions == null) return;

            var allActions = new List<string>();
            foreach (var actionMap in sourceInputActions.actionMaps)
            {
                foreach (var action in actionMap.actions)
                {
                    allActions.Add(action.name);
                }
            }

            // Smart mapping based on common naming patterns
            var mappingRules = new Dictionary<string, string[]>
            {
                { InputActionNames.MOVE, new[] { "Move", "Movement", "Walk", "WASD" } },
                { InputActionNames.FIRE, new[] { "Fire", "Attack", "Shoot", "Primary", "LeftClick" } },
                { InputActionNames.JUMP, new[] { "Jump", "Hop", "Space" } },
                { InputActionNames.CROUCH, new[] { "Crouch", "Duck", "Ctrl" } },
                { InputActionNames.SPRINT, new[] { "Sprint", "Run", "Shift", "Dash" } },
                { InputActionNames.RELOAD, new[] { "Reload", "R" } },
                { InputActionNames.INVENTORY, new[] { "Inventory", "Bag", "Items", "Tab", "I" } },
                { InputActionNames.INTERACT, new[] { "Interact", "Use", "E", "F" } },
                { InputActionNames.MAP, new[] { "Map", "M" } },
                { InputActionNames.PAUSE, new[] { "Pause", "Menu", "Escape", "ESC" } },
                { InputActionNames.BACK, new[] { "Back", "Cancel", "Escape", "ESC" } },
                { InputActionNames.LEFT, new[] { "Left", "LeftClick", "Primary" } },
                { InputActionNames.RIGHT, new[] { "Right", "RightClick", "Secondary" } },
                { InputActionNames.PRESS, new[] { "Press", "Click", "Touch", "Primary" } },
                { InputActionNames.POSITION, new[] { "Position", "Mouse", "Pointer", "Touch" } }
            };

            foreach (var rule in mappingRules)
            {
                var expectedAction = rule.Key;
                var possibleNames = rule.Value;

                foreach (var possibleName in possibleNames)
                {
                    var match = allActions.FirstOrDefault(a => 
                        a.Equals(possibleName, System.StringComparison.OrdinalIgnoreCase));
                    
                    if (match != null)
                    {
                        suggestedMappings[expectedAction] = match;
                        break;
                    }
                }
            }
        }

        private void AutoMapActions()
        {
            customMappings.Clear();
            foreach (var suggestion in suggestedMappings)
            {
                customMappings[suggestion.Key] = suggestion.Value;
            }
            Repaint();
        }

        private void CreateNewMappingAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Input Action Mapping",
                "InputActionMapping",
                "asset",
                "Create a new Input Action Mapping asset"
            );

            if (!string.IsNullOrEmpty(path))
            {
                var mapping = CreateInstance<InputActionMapping>();
                AssetDatabase.CreateAsset(mapping, path);
                AssetDatabase.SaveAssets();
                targetMapping = mapping;
            }
        }

        private void ApplyMappings()
        {
            if (targetMapping == null) return;

            var serializedObject = new SerializedObject(targetMapping);
            var mappingsProperty = serializedObject.FindProperty("actionMappings");
            
            mappingsProperty.ClearArray();

            var finalMappings = new Dictionary<string, string>(suggestedMappings);
            foreach (var custom in customMappings)
            {
                if (!string.IsNullOrEmpty(custom.Value))
                {
                    finalMappings[custom.Key] = custom.Value;
                }
            }

            int index = 0;
            foreach (var mapping in finalMappings)
            {
                mappingsProperty.InsertArrayElementAtIndex(index);
                var element = mappingsProperty.GetArrayElementAtIndex(index);
                
                element.FindPropertyRelative("expectedName").stringValue = mapping.Key;
                element.FindPropertyRelative("actualName").stringValue = mapping.Value;
                
                index++;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(targetMapping);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Success", "Action mappings applied successfully!", "OK");
        }

        private void ValidateMappings()
        {
            if (targetMapping == null || sourceInputActions == null) return;

            var result = targetMapping.ValidateMapping(sourceInputActions);
            
            EditorUtility.DisplayDialog(
                result.IsValid ? "Validation Successful" : "Validation Issues Found",
                result.GetReport(),
                "OK"
            );
        }
    }
}