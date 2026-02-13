using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Xunit;
using HarmonyLib;
using MageQuitModFramework.Utilities;

namespace MageQuitModFramework.Tests
{
    public class GameModificationHelpersTests
    {
        private class TestClass
        {
#pragma warning disable CS0414
            private int _privateInt = 42;
            private string _privateString = "test";
#pragma warning restore CS0414
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

        [Fact]
        public void ReplaceIntConstant_HandlesNegativeValues()
        {
            var oldValue = -10;
            var newValue = -20;
            var instructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldc_I4, oldValue),
                new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)-10)
            };

            var result = GameModificationHelpers.ReplaceIntConstant(instructions, oldValue, newValue).ToList();

            Assert.Equal(OpCodes.Ldc_I4, result[0].opcode);
            Assert.Equal(newValue, result[0].operand);
            Assert.Equal(OpCodes.Ldc_I4, result[1].opcode);
            Assert.Equal(newValue, result[1].operand);
        }

        [Fact]
        public void ReplaceIntConstant_HandlesAllSpecialOpcodes()
        {
            // Test replacing each special opcode individually
            for (int i = 0; i <= 8; i++)
            {
                var instructions = new List<CodeInstruction>();
                var expectedOpCode = i switch
                {
                    0 => OpCodes.Ldc_I4_0,
                    1 => OpCodes.Ldc_I4_1,
                    2 => OpCodes.Ldc_I4_2,
                    3 => OpCodes.Ldc_I4_3,
                    4 => OpCodes.Ldc_I4_4,
                    5 => OpCodes.Ldc_I4_5,
                    6 => OpCodes.Ldc_I4_6,
                    7 => OpCodes.Ldc_I4_7,
                    8 => OpCodes.Ldc_I4_8,
                    _ => OpCodes.Nop
                };
                instructions.Add(new CodeInstruction(expectedOpCode));

                var result = GameModificationHelpers.ReplaceIntConstant(instructions, i, 100).ToList();

                Assert.Equal(OpCodes.Ldc_I4, result[0].opcode);
                Assert.Equal(100, result[0].operand);
            }
        }

        [Fact]
        public void ReplaceIntConstant_DoesNotReplaceNonMatchingInstructions()
        {
            var oldValue = 42;
            var instructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldc_I4, 99),  // Different value
                new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)50),  // Different value
                new CodeInstruction(OpCodes.Ldc_R4, 42f),  // Different type
                new CodeInstruction(OpCodes.Nop)  // Different opcode
            };

            var result = GameModificationHelpers.ReplaceIntConstant(instructions, oldValue, 999).ToList();

            Assert.Equal(OpCodes.Ldc_I4, result[0].opcode);
            Assert.Equal(99, result[0].operand);
            Assert.Equal(OpCodes.Ldc_I4_S, result[1].opcode);
            Assert.Equal((sbyte)50, result[1].operand);
            Assert.Equal(OpCodes.Ldc_R4, result[2].opcode);
            Assert.Equal(42f, result[2].operand);
            Assert.Equal(OpCodes.Nop, result[3].opcode);
        }

        [Fact]
        public void ReplaceIntConstant_HandlesSbyteBoundaries()
        {
            var maxSbyte = (sbyte)127;
            var minSbyte = (sbyte)-128;
            var instructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldc_I4_S, maxSbyte),
                new CodeInstruction(OpCodes.Ldc_I4_S, minSbyte)
            };

            var result1 = GameModificationHelpers.ReplaceIntConstant(instructions, 127, 200).ToList();
            Assert.Equal(OpCodes.Ldc_I4, result1[0].opcode);
            Assert.Equal(200, result1[0].operand);

            var result2 = GameModificationHelpers.ReplaceIntConstant(instructions, -128, -200).ToList();
            Assert.Equal(OpCodes.Ldc_I4, result2[1].opcode);
            Assert.Equal(-200, result2[1].operand);
        }

        [Fact]
        public void ReplaceIntConstant_ReplacesMultipleOccurrences()
        {
            var oldValue = 5;
            var newValue = 100;
            var instructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldc_I4_5),
                new CodeInstruction(OpCodes.Ldc_I4, oldValue),
                new CodeInstruction(OpCodes.Ldc_I4_5),
                new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)oldValue)
            };

            var result = GameModificationHelpers.ReplaceIntConstant(instructions, oldValue, newValue).ToList();

            // All should be replaced
            Assert.Equal(4, result.Count);
            foreach (var instr in result)
            {
                Assert.Equal(OpCodes.Ldc_I4, instr.opcode);
                Assert.Equal(newValue, instr.operand);
            }
        }

        [Fact]
        public void ReplaceIntConstant_HandlesEmptyInstructions()
        {
            var instructions = new List<CodeInstruction>();

            var result = GameModificationHelpers.ReplaceIntConstant(instructions, 1, 2).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void ReplaceIntConstant_ConvertsBetweenSpecialOpcodes()
        {
            var instructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Ldc_I4_2),
                new CodeInstruction(OpCodes.Ldc_I4_3),
                new CodeInstruction(OpCodes.Ldc_I4_4),
                new CodeInstruction(OpCodes.Ldc_I4_5),
                new CodeInstruction(OpCodes.Ldc_I4_6),
                new CodeInstruction(OpCodes.Ldc_I4_7),
                new CodeInstruction(OpCodes.Ldc_I4_8)
            };

            // Replace 3 with 7
            var result = GameModificationHelpers.ReplaceIntConstant(instructions, 3, 7).ToList();

            Assert.Equal(OpCodes.Ldc_I4_0, result[0].opcode);
            Assert.Equal(OpCodes.Ldc_I4_1, result[1].opcode);
            Assert.Equal(OpCodes.Ldc_I4_2, result[2].opcode);
            Assert.Equal(OpCodes.Ldc_I4_7, result[3].opcode);  // Changed
            Assert.Equal(OpCodes.Ldc_I4_4, result[4].opcode);
            Assert.Equal(OpCodes.Ldc_I4_5, result[5].opcode);
            Assert.Equal(OpCodes.Ldc_I4_6, result[6].opcode);
            Assert.Equal(OpCodes.Ldc_I4_7, result[7].opcode);
            Assert.Equal(OpCodes.Ldc_I4_8, result[8].opcode);
        }

        [Fact]
        public void ReplaceIntConstant_HandlesLargeValues()
        {
            var oldValue = 1000000;
            var newValue = 2000000;
            var instructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldc_I4, oldValue)
            };

            var result = GameModificationHelpers.ReplaceIntConstant(instructions, oldValue, newValue).ToList();

            Assert.Equal(OpCodes.Ldc_I4, result[0].opcode);
            Assert.Equal(newValue, result[0].operand);
        }

        [Fact]
        public void ReplaceIntConstant_CoversAllSpecialOpcodeReplacements()
        {
            // Test replacing FROM value 6 TO various values (0-8)
            for (int newVal = 0; newVal <= 8; newVal++)
            {
                var instructions = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldc_I4_6)
                };

                var result = GameModificationHelpers.ReplaceIntConstant(instructions, 6, newVal).ToList();

                var expectedOpCode = newVal switch
                {
                    0 => OpCodes.Ldc_I4_0,
                    1 => OpCodes.Ldc_I4_1,
                    2 => OpCodes.Ldc_I4_2,
                    3 => OpCodes.Ldc_I4_3,
                    4 => OpCodes.Ldc_I4_4,
                    5 => OpCodes.Ldc_I4_5,
                    6 => OpCodes.Ldc_I4_6,
                    7 => OpCodes.Ldc_I4_7,
                    8 => OpCodes.Ldc_I4_8,
                    _ => OpCodes.Nop
                };
                Assert.Equal(expectedOpCode, result[0].opcode);
                Assert.Null(result[0].operand);
            }
        }

        [Fact]
        public void ReplaceIntConstant_ReplacesLdc_I4_WithAllSpecialOpcodes()
        {
            // Test Ldc_I4 (with operand) being replaced with special opcodes 0-8
            for (int newVal = 0; newVal <= 8; newVal++)
            {
                var instructions = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldc_I4, 100)
                };

                var result = GameModificationHelpers.ReplaceIntConstant(instructions, 100, newVal).ToList();

                var expectedOpCode = newVal switch
                {
                    0 => OpCodes.Ldc_I4_0,
                    1 => OpCodes.Ldc_I4_1,
                    2 => OpCodes.Ldc_I4_2,
                    3 => OpCodes.Ldc_I4_3,
                    4 => OpCodes.Ldc_I4_4,
                    5 => OpCodes.Ldc_I4_5,
                    6 => OpCodes.Ldc_I4_6,
                    7 => OpCodes.Ldc_I4_7,
                    8 => OpCodes.Ldc_I4_8,
                    _ => OpCodes.Nop
                };
                Assert.Equal(expectedOpCode, result[0].opcode);
                Assert.Null(result[0].operand);
            }
        }

        [Fact]
        public void ReplaceIntConstant_Ldc_I4_S_WithValue6_ToSpecialOpcode()
        {
            // Specifically test Ldc_I4_S with value 6 being replaced to special opcode
            // This ensures line 66 (matching oldValue == 6) and line 87 (setting operand = null) are covered
            var instructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)6)
            };

            var result = GameModificationHelpers.ReplaceIntConstant(instructions, 6, 3).ToList();

            Assert.Equal(OpCodes.Ldc_I4_3, result[0].opcode);
            Assert.Null(result[0].operand);
        }

        [Fact]
        public void ReplaceIntConstant_SpecialOpcode6_Explicitly()
        {
            // Explicitly test the special opcode path for value 6
            // to ensure line 66 in the oldValue switch is covered
            var instructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldc_I4_6)  // Special opcode for 6
            };

            // Replace 6 with 9 (forces use of Ldc_I4)
            var result = GameModificationHelpers.ReplaceIntConstant(instructions, 6, 9).ToList();

            Assert.Equal(OpCodes.Ldc_I4, result[0].opcode);
            Assert.Equal(9, result[0].operand);
        }

        [Fact]
        public void ReplaceIntConstant_Ldc_I4_With6_ToSpecialOpcode()
        {
            // Test Ldc_I4 with value 6 being replaced to special opcode
            // to ensure line 87 (setting operand = null) is covered
            var instructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldc_I4, 6)
            };

            var result = GameModificationHelpers.ReplaceIntConstant(instructions, 6, 2).ToList();

            Assert.Equal(OpCodes.Ldc_I4_2, result[0].opcode);
            Assert.Null(result[0].operand);  // operand should be set to null
        }

        [Fact]
        public void ApplyFieldValuesToInstance_UpdatesFloatFields()
        {
            var obj = new TestClass();
            var values = new Dictionary<string, float>
            {
                { "PublicFloat", 6.28f }
            };

            GameModificationHelpers.ApplyFieldValuesToInstance(obj, values);

            Assert.Equal(6.28f, obj.PublicFloat);
        }

        [Fact]
        public void ApplyFieldValuesToInstance_HandlesNullInstance()
        {
            var values = new Dictionary<string, float> { { "field", 1.0f } };

            // Should not throw
            GameModificationHelpers.ApplyFieldValuesToInstance(null, values);
        }

        [Fact]
        public void ApplyFieldValuesToInstance_HandlesNullValues()
        {
            var obj = new TestClass();

            // Should not throw
            GameModificationHelpers.ApplyFieldValuesToInstance(obj, null);
        }

        [Fact]
        public void ApplyFieldValuesToInstance_IgnoresNonExistentFields()
        {
            var obj = new TestClass();
            var values = new Dictionary<string, float>
            {
                { "PublicFloat", 1.5f },
                { "NonExistentField", 999f }
            };

            GameModificationHelpers.ApplyFieldValuesToInstance(obj, values);

            Assert.Equal(1.5f, obj.PublicFloat);
        }
    }
}
