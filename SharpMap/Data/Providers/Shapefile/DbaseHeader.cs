// Copyright 2006, 2007 - Rory Plaire (codekaizen@gmail.com)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;

namespace SharpMap.Data.Providers.ShapeFile
{
    /// <summary>
    /// Represents the header of an xBase file.
    /// </summary>
    internal class DbaseHeader
    {
        private DateTime _lastUpdate = DateTime.Now;
        private UInt32 _numberOfRecords;
        private Int16 _headerLength;
        private Int16 _recordLength;
        private readonly byte _languageDriver;
        //private DbaseField[] _dbaseColumns;
    	private readonly Dictionary<string, DbaseField> _dbaseColumns = new Dictionary<string, DbaseField>();

        internal DbaseHeader(byte languageDriverCode, DateTime lastUpdate, UInt32 numberOfRecords)
        {
            _languageDriver = languageDriverCode;
            _lastUpdate = lastUpdate;
            _numberOfRecords = numberOfRecords;
        }

        /// <summary>
        /// Gets a value which indicates which code page text data is 
        /// stored in.
        /// </summary>
        internal byte LanguageDriver
        {
            get { return _languageDriver; }
        }

        internal DateTime LastUpdate
        {
            get { return _lastUpdate; }
            set { _lastUpdate = value; }
        }

        internal UInt32 RecordCount
        {
            get { return _numberOfRecords; }
            set { _numberOfRecords = value; }
        }

        internal ICollection<DbaseField> Columns
        {
            get { return _dbaseColumns.Values; }
            set
            {
				_dbaseColumns.Clear();

				RecordLength = 1; // Deleted flag length

            	foreach (DbaseField field in value)
            	{
					_dbaseColumns.Add(field.ColumnName, field);
					RecordLength += field.Length;
            	}

                HeaderLength = (short)((DbaseConstants.ColumnDescriptionLength * _dbaseColumns.Count)
                    + DbaseConstants.ColumnDescriptionOffset + 1 /* For terminator */);
            }
        }

        internal Int16 HeaderLength
        {
            get { return _headerLength; }
            private set { _headerLength = value; }
        }

        internal Int16 RecordLength
        {
            get { return _recordLength; }
            private set { _recordLength = value; }
        }

        internal Encoding FileEncoding
        {
            get { return DbaseLocaleRegistry.GetEncoding(LanguageDriver); }
        }

        public override string ToString()
        {
            return String.Format("[DbaseHeader] Records: {0}; Columns: {1}; Last Update: {2}; " +
                "Record Length: {3}; Header Length: {4}", RecordCount, Columns.Count,
                LastUpdate, RecordLength, HeaderLength);
        }

        /// <summary>
        /// Returns a DataTable that describes the column metadata of the DBase file.
        /// </summary>
        /// <returns>A DataTable that describes the column metadata.</returns>
        internal DataTable GetSchemaTable()
        {
            DataTable schema = ProviderSchemaHelper.CreateSchemaTable();

        	foreach (KeyValuePair<string, DbaseField> entry in _dbaseColumns)
        	{
                DataRow r = schema.NewRow();
        		DbaseField column = entry.Value;
				r[ProviderSchemaHelper.ColumnNameColumn] = entry.Key;
                r[ProviderSchemaHelper.ColumnSizeColumn] = column.Length;
				r[ProviderSchemaHelper.ColumnOrdinalColumn] = column.Ordinal;
				r[ProviderSchemaHelper.NumericPrecisionColumn] = column.Decimals;
                r[ProviderSchemaHelper.NumericScaleColumn] = 0;
				r[ProviderSchemaHelper.DataTypeColumn] = column.DataType;
                r[ProviderSchemaHelper.AllowDBNullColumn] = true;
                r[ProviderSchemaHelper.IsReadOnlyColumn] = true;
                r[ProviderSchemaHelper.IsUniqueColumn] = false;
                r[ProviderSchemaHelper.IsRowVersionColumn] = false;
                r[ProviderSchemaHelper.IsKeyColumn] = false;
                r[ProviderSchemaHelper.IsAutoIncrementColumn] = false;
                r[ProviderSchemaHelper.IsLongColumn] = false;

                // specializations, if ID is unique
                //if (_ColumnNames[i] == "ID")
                //	r["IsUnique"] = true;

                schema.Rows.Add(r);
            }

            return schema;
        }

        internal static DbaseHeader ParseDbfHeader(Stream dataStream)
        {
            DbaseHeader header;

            using (BinaryReader reader = new BinaryReader(dataStream))
            {
                if (reader.ReadByte() != DbaseConstants.DbfVersionCode)
                {
                    throw new NotSupportedException("Unsupported DBF Type");
                }

                DateTime lastUpdate = new DateTime(reader.ReadByte() + DbaseConstants.DbaseEpoch,
                    reader.ReadByte(), reader.ReadByte()); //Read the last update date
                UInt32 recordCount = reader.ReadUInt32(); // read number of records.
                Int16 storedHeaderLength = reader.ReadInt16(); // read length of header structure.
                Int16 storedRecordLength = reader.ReadInt16(); // read length of a record
                reader.BaseStream.Seek(DbaseConstants.EncodingOffset, SeekOrigin.Begin); //Seek to encoding flag
                Byte languageDriver = reader.ReadByte(); //Read and parse Language driver
                reader.BaseStream.Seek(DbaseConstants.ColumnDescriptionOffset, SeekOrigin.Begin); //Move past the reserved bytes
                Int32 numberOfColumns = (storedHeaderLength - DbaseConstants.ColumnDescriptionOffset) /
                    DbaseConstants.ColumnDescriptionLength;  // calculate the number of DataColumns in the header

                header = new DbaseHeader(languageDriver, lastUpdate, recordCount);

                DbaseField[] columns = new DbaseField[numberOfColumns];
            	int offset = 1;

                for (Int32 i = 0; i < columns.Length; i++)
                {
                    String colName = header.FileEncoding.GetString(
                        reader.ReadBytes(11)).Replace("\0", "").Trim();

                    Char fieldtype = reader.ReadChar();

                    // Unused address data...
                    reader.ReadInt32();

                    Int16 fieldLength = reader.ReadByte();
                    Byte decimals = 0;

                    if (fieldtype == 'N' || fieldtype == 'F')
                    {
                        decimals = reader.ReadByte();
                    }
                    else
                    {
                        fieldLength += (short)(reader.ReadByte() << 8);
                    }

                    Type dataType = mapFieldTypeToClrType(fieldtype, decimals, fieldLength);

                    columns[i] = new DbaseField(header, colName, dataType, fieldLength, decimals, i, offset);

                	offset += fieldLength;

                    // Move stream to next field record
                    reader.BaseStream.Seek(DbaseConstants.BytesFromEndOfDecimalInFieldRecord, SeekOrigin.Current);
                }

                header.Columns = columns;

                if (storedHeaderLength != header.HeaderLength)
                {
                    throw new InvalidDbaseFileException(
                        "Recorded header length doesn't equal computed header length.");
                }

                if (storedRecordLength != header.RecordLength)
                {
                    throw new InvalidDbaseFileException(
                        "Recorded record length doesn't equal computed record length.");
                }
            }

            return header;
        }

        private static Type mapFieldTypeToClrType(Char fieldtype, Byte decimals, Int16 length)
        {
            Type dataType;

            switch (fieldtype)
            {
                case 'L':
                    return typeof(bool);
                    break;
                case 'C':
                    return typeof(string);
                    break;
                case 'D':
                    return typeof(DateTime);
                    break;
                case 'N':
                    // If the number doesn't have any decimals, 
                    // make the type an integer, if possible
                    if (decimals == 0)
                    {
                        if (length <= 4) return typeof(Int16);
                        else if (length <= 9) return typeof(Int32);
                        else if (length <= 18) return typeof(Int64);
                        else return typeof(double);
                    }
                    else
                    {
                        return typeof(double);
                    }
                case 'F':
                    return typeof(float);
                case 'B':
                    return typeof(byte[]);
                default:
                    return null;
            }
        }
    }
}
