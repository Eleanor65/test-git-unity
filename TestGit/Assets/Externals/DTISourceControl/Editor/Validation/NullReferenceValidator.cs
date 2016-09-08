using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DTI.SourceControl.Validation
{
    public class NullRefferenceValidator : ICommitValidator
    {
        public bool IsValid(IEnumerable<string> files, out IEnumerable<string> errorMessages)
        {
            var errors = new List<string>();
            var isValid = true;

            foreach (var file in files)
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(file);
                if (asset == null)
                    continue;

                var objects = GetChildrenRecursively(asset);
                objects.Add(asset);
                foreach (var gameObject in objects)
                {
                    var components = gameObject.GetComponents<MonoBehaviour>();

                    foreach (var component in components)
                    {
                        var serializedObject = new SerializedObject(component);
                        var property = serializedObject.GetIterator();
                        var next = property.NextVisible(true);
                        while (next)
                        {
                            if (property.propertyType == SerializedPropertyType.ObjectReference)
                            {
                                var item = property.objectReferenceValue;
                                if (item == null)
                                {
                                    isValid = false;
                                    errors.Add(
                                        String.Format(
                                            "Reference equals null. Path: {0}; Object: {1}; Component: {2}; Property: {3}",
                                            file, gameObject.name, component.GetType().Name, property.name));
                                }
                            }
                            next = property.NextVisible(false);
                        }
                    }
                }
            }

            errorMessages = errors;
            return isValid;
        }

        private List<GameObject> GetChildrenRecursively(GameObject go)
        {
            var children = new List<GameObject>();
            for (int i = 0; i < go.transform.childCount; i++)
            {
                var child = go.transform.GetChild(i).gameObject;
                children.Add(child);
                children.AddRange(GetChildrenRecursively(child));
            }
            return children;
        }
    }
}