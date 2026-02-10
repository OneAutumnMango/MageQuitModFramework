using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Xunit;
using HarmonyLib;
using SpellcastModFramework.Core;

namespace SpellcastModFramework.Tests
{
    public class GameModificationHelpersTests
    {
        private class TestClass
        {
            private int _privateInt = 42;
            private string _privateString = "test";
            public float PublicFloat = 3.14f;
        }

        [Fact]
        public void GetPrivateField_ReturnsCorrectIntValue()
        {
            var obj = new TestClass();
            
            var result = GameModificationHelpers.GetPrivateField<int>(obj, "_privateInt");
            
            Assert.Equal(42, result);
        }

        [Fact]
        public void GetPrivateField_ReturnsCorrectStringValue()
        {
            var obj = new TestClass();
            
            var result = GameModificationHelpers.GetPrivateField<string>(obj, "_privateString");
            
            Assert.Equal("test", result);
        }

        [Fact]
        public void SetPrivateField_UpdatesIntValue()
        {
            var obj = new TestClass();
            var expected = 100;
            
            GameModificationHelpers.SetPrivateField(obj, "_privateInt", expected);
            var result = GameModificationHelpers.GetPrivateField<int>(obj, "_privateInt");
            
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SetPrivateField_UpdatesStringValue()
        {
            var obj = new TestClass();
            var expected = "updated";
            
            GameModificationHelpers.SetPrivateField(obj, "_privateString", expected);
            var result = GameModificationHelpers.GetPrivateField<string>(obj, "_privateString");
            
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetPrivateField_ReturnsCorrectFloatValue()
        {
            var obj = new TestClass();
            
            var result = GameModificationHelpers.GetPrivateField<float>(obj, "PublicFloat");
            
            Assert.Equal(3.14f, result);
        }

        [Fact]
        public void SetPrivateField_UpdatesFloatValue()
        {
            var obj = new TestClass();
            var expected = 9.99f;
            
            GameModificationHelpers.SetPrivateField(obj, "PublicFloat", expected);
            var result = GameModificationHelpers.GetPrivateField<float>(obj, "PublicFloat");
            
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SetMultipleFields_UpdatesAllFields()
        {
            var obj = new TestClass();
            var updates = new Dictionary<string, object>
            {
                { "_privateInt", 999 },
                { "_privateString", "multi" },
                { "PublicFloat", 2.71f }
            };
            
            GameModificationHelpers.SetMultipleFields(obj, updates);
            
            Assert.Equal(updates["_privateInt"], GameModificationHelpers.GetPrivateField<int>(obj, "_privateInt"));
            Assert.Equal(updates["_privateString"], GameModificationHelpers.GetPrivateField<string>(obj, "_privateString"));
            Assert.Equal(updates["PublicFloat"], obj.PublicFloat);
        }

        [Fact]
        public void SetMultipleFields_IgnoresNonExistentFields()
        {
            var obj = new TestClass();
            var updates = new Dictionary<string, object>
            {
                { "_privateInt", 50 },
                { "NonExistentField", "value" }
            };
            
            GameModificationHelpers.SetMultipleFields(obj, updates);
            
            Assert.Equal(updates["_privateInt"], GameModificationHelpers.GetPrivateField<int>(obj, "_privateInt"));
        }

        [Fact]
        public void ReplaceFloatConstant_ReplacesMatchingValue()
        {
            var oldValue = 3.14f;
            var newValue = 99.9f;
            var unchanged = 2.71f;
            var instructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldc_R4, oldValue),
                new CodeInstruction(OpCodes.Ldc_R4, unchanged),
                new CodeInstruction(OpCodes.Ldc_R4, oldValue)
            };

            var result = GameModificationHelpers.ReplaceFloatConstant(instructions, oldValue, newValue).ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal(newValue, result[0].operand);
            Assert.Equal(unchanged, result[1].operand);
            Assert.Equal(newValue, result[2].operand);
        }

        [Fact]
        public void ReplaceFloatConstant_RespectsTolerance()
        {
            var oldValue = 1.0f;
            var newValue = 5.0f;
            var tolerance = 0.001f;
            var withinTolerance = 1.0000001f;
            var outOfTolerance = 1.01f;
            var instructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldc_R4, withinTolerance),
                new CodeInstruction(OpCodes.Ldc_R4, outOfTolerance)
            };

            var result = GameModificationHelpers.ReplaceFloatConstant(instructions, oldValue, newValue, tolerance).ToList();

            Assert.Equal(newValue, result[0].operand);
            Assert.Equal(outOfTolerance, result[1].operand);
        }

        [Fact]
        public void ReplaceFloatConstant_DoesNotReplaceNonMatching()
        {
            var existingFloat = 1.5f;
            var existingInt = 42;
            var instructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldc_R4, existingFloat),
                new CodeInstruction(OpCodes.Ldc_I4, existingInt)
            };

            var result = GameModificationHelpers.ReplaceFloatConstant(instructions, 2.5f, 99.9f).ToList();

            Assert.Equal(existingFloat, result[0].operand);
            Assert.Equal(existingInt, result[1].operand);
        }

        [Fact]
        public void ReplaceIntConstant_ReplacesStandardInt()
        {
            var oldValue = 100;
            var newValue = 999;
            var unchanged = 200;
            var instructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldc_I4, oldValue),
                new CodeInstruction(OpCodes.Ldc_I4, unchanged)
            };

            var result = GameModificationHelpers.ReplaceIntConstant(instructions, oldValue, newValue).ToList();

            Assert.Equal(newValue, result[0].operand);
            Assert.Equal(unchanged, result[1].operand);
        }

        [Fact]
        public void ReplaceIntConstant_ReplacesShortForm()
        {
            var oldValue = 50;
            var newValue = 999;
            var instructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)oldValue)
            };

            var result = GameModificationHelpers.ReplaceIntConstant(instructions, oldValue, newValue).ToList();

            Assert.Equal(OpCodes.Ldc_I4, result[0].opcode);
            Assert.Equal(newValue, result[0].operand);
        }

        [Fact]
        public void ReplaceIntConstant_ReplacesSpecialOpcodes_0Through8()
        {
            var oldValue = 5;
            var newValue = 100;
            var instructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Ldc_I4_5),
                new CodeInstruction(OpCodes.Ldc_I4_8)
            };

            var result = GameModificationHelpers.ReplaceIntConstant(instructions, oldValue, newValue).ToList();

            Assert.Equal(OpCodes.Ldc_I4_0, result[0].opcode);
            Assert.Equal(OpCodes.Ldc_I4_1, result[1].opcode);
            Assert.Equal(OpCodes.Ldc_I4, result[2].opcode);
            Assert.Equal(newValue, result[2].operand);
            Assert.Equal(OpCodes.Ldc_I4_8, result[3].opcode);
        }

        [Fact]
        public void ReplaceIntConstant_ConvertsToSpecialOpcode_WhenNewValueIs0Through8()
        {
            var oldValue = 100;
            var newValue = 3;
            var instructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldc_I4, oldValue)
            };

            var result = GameModificationHelpers.ReplaceIntConstant(instructions, oldValue, newValue).ToList();

            Assert.Equal(OpCodes.Ldc_I4_3, result[0].opcode);
            Assert.Null(result[0].operand);
        }

        [Fact]
        public void ReplaceIntConstant_HandlesEdgeCases()
        {
            var value9 = 9;
            var instructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Ldc_I4_8),
                new CodeInstruction(OpCodes.Ldc_I4, value9)
            };

            var result1 = GameModificationHelpers.ReplaceIntConstant(instructions, 0, 7).ToList();
            Assert.Equal(OpCodes.Ldc_I4_7, result1[0].opcode);

            var result2 = GameModificationHelpers.ReplaceIntConstant(instructions, 8, 0).ToList();
            Assert.Equal(OpCodes.Ldc_I4_0, result2[1].opcode);

            var result3 = GameModificationHelpers.ReplaceIntConstant(instructions, value9, 10).ToList();
            Assert.Equal(OpCodes.Ldc_I4, result3[2].opcode);
            Assert.Equal(10, result3[2].operand);
        }
    }
}
