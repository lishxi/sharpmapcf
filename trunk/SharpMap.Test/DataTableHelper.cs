using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using NUnit.Framework;

namespace SharpMap.Tests
{
	static class DataTableHelper
	{
		public static void AssertTableStructureIdentical(DataTable lhs, DataTable rhs)
		{
			Assert.AreNotSame(lhs, rhs);
			Assert.Greater(lhs.Columns.Count, 0);
			Assert.AreEqual(lhs.Columns.Count, rhs.Columns.Count);

			for (int i = 0; i < lhs.Columns.Count; i++)
			{
				Assert.AreEqual(lhs.Columns[i].AllowDBNull, rhs.Columns[i].AllowDBNull);
				Assert.AreEqual(lhs.Columns[i].AutoIncrement, rhs.Columns[i].AutoIncrement);
				Assert.AreEqual(lhs.Columns[i].AutoIncrementSeed, rhs.Columns[i].AutoIncrementSeed);
				Assert.AreEqual(lhs.Columns[i].AutoIncrementStep, rhs.Columns[i].AutoIncrementStep);
				Assert.AreEqual(lhs.Columns[i].Caption, rhs.Columns[i].Caption);
				Assert.AreEqual(lhs.Columns[i].ColumnMapping, rhs.Columns[i].ColumnMapping);
				Assert.AreEqual(lhs.Columns[i].ColumnName, rhs.Columns[i].ColumnName);
				Assert.AreEqual(lhs.Columns[i].DataType, rhs.Columns[i].DataType);
				Assert.AreEqual(lhs.Columns[i].DateTimeMode, rhs.Columns[i].DateTimeMode);
				Assert.AreEqual(lhs.Columns[i].DefaultValue, rhs.Columns[i].DefaultValue);
				Assert.AreEqual(lhs.Columns[i].Expression, rhs.Columns[i].Expression);
				Assert.AreEqual(lhs.Columns[i].MaxLength, rhs.Columns[i].MaxLength);
				Assert.AreEqual(lhs.Columns[i].Namespace, rhs.Columns[i].Namespace);
				Assert.AreEqual(lhs.Columns[i].Ordinal, rhs.Columns[i].Ordinal);
				Assert.AreEqual(lhs.Columns[i].Prefix, rhs.Columns[i].Prefix);
				Assert.AreEqual(lhs.Columns[i].ReadOnly, rhs.Columns[i].ReadOnly);
				Assert.AreEqual(lhs.Columns[i].Unique, rhs.Columns[i].Unique);
				Assert.AreEqual(lhs.Columns[i].ExtendedProperties.Count, rhs.Columns[i].ExtendedProperties.Count);

				object[] lhsProperties = new object[lhs.Columns[i].ExtendedProperties.Count];
				lhs.Columns[i].ExtendedProperties.CopyTo(lhsProperties, 0);

				object[] rhsProperties = new object[rhs.Columns[i].ExtendedProperties.Count];
				rhs.Columns[i].ExtendedProperties.CopyTo(rhsProperties, 0);

				Assert.AreEqual(lhsProperties.Length, rhsProperties.Length);

				for (int epIndex = 0; epIndex < lhsProperties.Length; epIndex++)
				{
					Assert.AreEqual(lhsProperties[epIndex], rhsProperties[epIndex]);
				}
			}

			Assert.AreEqual(lhs.PrimaryKey.Length, rhs.PrimaryKey.Length);

			if (lhs.PrimaryKey.Length > 0)
			{
				for (int i = 0; i < lhs.PrimaryKey.Length; i++)
				{
					Assert.AreEqual(lhs.PrimaryKey[i].AllowDBNull, rhs.PrimaryKey[i].AllowDBNull);
					Assert.AreEqual(lhs.PrimaryKey[i].AutoIncrement, rhs.PrimaryKey[i].AutoIncrement);
					Assert.AreEqual(lhs.PrimaryKey[i].AutoIncrementSeed, rhs.PrimaryKey[i].AutoIncrementSeed);
					Assert.AreEqual(lhs.PrimaryKey[i].AutoIncrementStep, rhs.PrimaryKey[i].AutoIncrementStep);
					Assert.AreEqual(lhs.PrimaryKey[i].Caption, rhs.PrimaryKey[i].Caption);
					Assert.AreEqual(lhs.PrimaryKey[i].ColumnMapping, rhs.PrimaryKey[i].ColumnMapping);
					Assert.AreEqual(lhs.PrimaryKey[i].ColumnName, rhs.PrimaryKey[i].ColumnName);
					Assert.AreEqual(lhs.PrimaryKey[i].DataType, rhs.PrimaryKey[i].DataType);
					Assert.AreEqual(lhs.PrimaryKey[i].DateTimeMode, rhs.PrimaryKey[i].DateTimeMode);
					Assert.AreEqual(lhs.PrimaryKey[i].DefaultValue, rhs.PrimaryKey[i].DefaultValue);
					Assert.AreEqual(lhs.PrimaryKey[i].Expression, rhs.PrimaryKey[i].Expression);
					Assert.AreEqual(lhs.PrimaryKey[i].MaxLength, rhs.PrimaryKey[i].MaxLength);
					Assert.AreEqual(lhs.PrimaryKey[i].Namespace, rhs.PrimaryKey[i].Namespace);
					Assert.AreEqual(lhs.PrimaryKey[i].Ordinal, rhs.PrimaryKey[i].Ordinal);
					Assert.AreEqual(lhs.PrimaryKey[i].Prefix, rhs.PrimaryKey[i].Prefix);
					Assert.AreEqual(lhs.PrimaryKey[i].ReadOnly, rhs.PrimaryKey[i].ReadOnly);
					Assert.AreEqual(lhs.PrimaryKey[i].Unique, rhs.PrimaryKey[i].Unique);
					Assert.AreEqual(lhs.PrimaryKey[i].ExtendedProperties.Count, rhs.PrimaryKey[i].ExtendedProperties.Count);

					object[] lhsProperties = new object[lhs.PrimaryKey[i].ExtendedProperties.Count];
					lhs.PrimaryKey[i].ExtendedProperties.CopyTo(lhsProperties, 0);

					object[] rhsProperties = new object[rhs.PrimaryKey[i].ExtendedProperties.Count];
					rhs.PrimaryKey[i].ExtendedProperties.CopyTo(rhsProperties, 0);

					for (int epIndex = 0; epIndex < lhsProperties.Length; epIndex++)
					{
						Assert.AreEqual(lhsProperties[epIndex], rhsProperties[epIndex]);
					}
				}
			}

			for (int i = 0; i < lhs.Constraints.Count; i++)
			{
				Assert.AreEqual(lhs.Constraints[i].ConstraintName, rhs.Constraints[i].ConstraintName);

				object[] lhsProperties = new object[lhs.Constraints[i].ExtendedProperties.Count];
				lhs.Columns[i].ExtendedProperties.CopyTo(lhsProperties, 0);

				object[] rhsProperties = new object[rhs.Constraints[i].ExtendedProperties.Count];
				rhs.Columns[i].ExtendedProperties.CopyTo(rhsProperties, 0);

				for (int epIndex = 0; epIndex < lhsProperties.Length; epIndex++)
				{
					Assert.AreEqual(lhsProperties[epIndex], rhsProperties[epIndex]);
				}
			}
		}
	}
}
