﻿using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
	public class NumberValidatorTests
	{
		[TestCase(0, 0, true, TestName = "ZeroPrecision")]
		[TestCase(-1, 0, true, TestName = "PrecisionLessThanZero")]
		[TestCase(1, 1, true, TestName = "ScaleEqualToPrecision")]
		[TestCase(1, 2, true, TestName = "ScaleGreaterThanPrecision")]
		[TestCase(1, -2, true, TestName = "NegativeScale")]
		public void NumberValidator_WithIncorrectScaleAndPrecision_ThrowException(
			int precision, int scale, bool onlyPositive)
		{
			Action action = () => new NumberValidator(precision, scale, onlyPositive);
			action.Should().Throw<ArgumentException>();
		}

		[TestCase(1, 0, true, TestName = "ZeroScale")]
		[TestCase(2, 1, true, TestName = "ScaleLessThanPrecision")]
		public void NumberValidator_WithCorrectScaleAndPrecision_DoesNotThrowException(
			int precision, int scale, bool onlyPositive)
		{
			Action action = () => new NumberValidator(precision, scale, onlyPositive);
			action.Should().NotThrow();
		}

		[TestCase(1, 0, true, null, TestName = "NullValue")]
		[TestCase(1, 0, true, "", TestName = "EmptyValue")]
		[TestCase(5, 2, true, "++1.23", TestName = "TwoSigns")]
		[TestCase(6, 2, true, "+++1.23", TestName = "MoreTwoSigns")]
		[TestCase(3, 2, true, "1..23", TestName = "TwoSeparators")]
		[TestCase(3, 2, true, "1..,23", TestName = "MoreTwoSeparators")]
		[TestCase(3, 2, true, "1.2.3", TestName = "SeparatorsInDifferentPositions")]
		[TestCase(3, 2, true, "1.bc", TestName = "CharsInFractionalPart")]
		[TestCase(3, 2, true, "a.23", TestName = "CharsInIntegerPart")]
		[TestCase(3, 2, true, "a.bc", TestName = "OnlyChars")]
		[TestCase(3, 2, true, "+1.23", TestName = "IntegerPartGreaterThanPrecision")]
		[TestCase(4, 2, true, "1.234", TestName = "FractionalPartGreaterThanScale")]
		[TestCase(4, 2, true, "-1.23", TestName = "NegativeNumberWithOnlyPositiveTrue")]
		public void IsValidNumber_ReturnsFalse_On(
			int precision, int scale, bool onlyPositive, string value)
		{
			var numberValidator = new NumberValidator(precision, scale, onlyPositive);
			numberValidator.IsValidNumber(value).Should().BeFalse();
		}

		[TestCase(1, 0, true, "0", TestName = "OnlyIntegerPart")]
		[TestCase(2, 1, true, "0.0", TestName = "NumberWithFractionalPart")]
		[TestCase(4, 2, true, "+1.23", TestName = "NumberWithSign")]
		[TestCase(4, 2, false, "+1.23", TestName = "PositiveNumberWithOnlyPositiveFalse")]
		public void IsValidNumber_ReturnsTrue_On(
			int precision, int scale, bool onlyPositive, string value)
		{
			var numberValidator = new NumberValidator(precision, scale, onlyPositive);
			numberValidator.IsValidNumber(value).Should().BeTrue();
		}
	}

	public class NumberValidator
	{
		private readonly Regex numberRegex;
		private readonly bool onlyPositive;
		private readonly int precision;
		private readonly int scale;

		public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
		{
			this.precision = precision;
			this.scale = scale;
			this.onlyPositive = onlyPositive;
			if (precision <= 0)
				throw new ArgumentException("precision must be a positive number");
			if (scale < 0 || scale >= precision)
				throw new ArgumentException("precision must be a non-negative number less or equal than precision");
			numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
		}

		public bool IsValidNumber(string value)
		{
			// Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
			// описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
			// Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
			// целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
			// Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

			if (string.IsNullOrEmpty(value))
				return false;

			var match = numberRegex.Match(value);
			if (!match.Success)
				return false;

			// Знак и целая часть
			var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
			// Дробная часть
			var fracPart = match.Groups[4].Value.Length;

			if (intPart + fracPart > precision || fracPart > scale)
				return false;

			if (onlyPositive && match.Groups[1].Value == "-")
				return false;
			return true;
		}
	}
}