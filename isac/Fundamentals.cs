using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Isac.Isql;

using System.IO;
using System.IO.Compression;
using Isac.Isql.Collections;

namespace Isac
{
	internal class Fundamentals : Parser
	{
		public Fundamentals()
		{
			
		}
		
		internal static string[] idataTypes = { "int", "float", "varchar", "char", "text", "datetime", "date", "time", "datetimespan", "timespan", "year" };

		internal void DublicatesChecker(string[] columns)
		{
			string[] lowers = new string[columns.Length];

			for (int i = 0; i < columns.Length; i++)
				lowers[i] = columns[i].ToLower();

			var duplicates = lowers.GroupBy(x => x)
					  .Where(g => g.Count() > 1)
					  .Select(y => y.Key)
					  .ToArray();
			var dupcount = lowers.GroupBy(x => x)
					  .Where(g => g.Count() > 1)
					  .Select(y => y.Count())
					  .ToArray();
			int n = (int)dupcount.Length;
			if (n != 0)
			{
				string col = "";

				col = string.Join(", ", duplicates);

				throw new Exception($"{col} are duplicated column(s)");
			}
		}

		internal void SeeDataType(string[] data_types)
		{
			int types = 0;
			for (int i = 0; i < data_types.Length; i++)
			{
				int a = types;
				string currentType = data_types[i];
				for (int j = 0; j < idataTypes.Length; j++)
				{
					if (currentType == idataTypes[j])
					{
						types += 1;
						goto Next;
					}
				}
				if (a == types)
				{
					throw new Exception($"Error: '{data_types[i]}' is an unknown type used as a datatype");
				}
			Next:
				continue;
			}
		}

		internal void ColumnLengthDefination(string column_length)
		{
			char[] nums = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
			int clng = 0;

			for (int i = 0; i < column_length.Length; i++)
			{
				for (int j = 0; j < nums.Length; j++)
				{
					if (column_length[i] == nums[j])
						clng++;

				}
			}
			if (clng != column_length.Length)
			{
				throw new Exception("table size must be a number");
			}
		}

		internal void DefaultCheck(string value, string type)
		{
			if (type == "int")
			{
				try
				{
					int i = Convert.ToInt32(value);
				}
				catch (Exception)
				{
					throw new Exception($"Error: the value '{value}' cannot be set as a default value for an int data type");
				}
			}

			if (type == "float")
			{
				try
				{
					float i = Convert.ToSingle(value);
				}
				catch (Exception)
				{
					throw new Exception($"Error: the value '{value}' cannot be set as a default value for a float data type");
				}
			}

			if (type == "varchar")
			{

			}

			if (type == "char")
			{
				try
				{
					int i = Convert.ToChar(value);
				}
				catch (Exception e)
				{
					throw new Exception($"Error: the value '{value}' cannot be set as a default value for char data type\n{e.Message}");
				}
			}

			if (type == "text")
			{
				TextCheacker(value);
			}

			if (type == "datetime")
			{
				try
				{
					DateTime i = Convert.ToDateTime(value);
				}
				catch (Exception)
				{
					throw new Exception($"Error: the value '{value}' cannot be set as a default value for datatime data type");
				}
			}
		}

		internal void TextCheacker(string text)
		{
			string text_characters = "abcdefghijklmnopqrstuvwxyz_ ";
			foreach (char item in text)
			{
				if (text_characters.Contains(item.ToString().ToLower()) == false)
					throw new Exception($"Error: unacceptable character encountered\nChar: '{item}'\nText: '{text}'\nA text data type contains only alphabets no symbols or numbers are allowed");
			}
		}

		internal void DataTypeConverter(string[] tableDataTypes, string[] insertDataTypes, string[] insertValues)
		{
			int[] test = Enumerable.Repeat(0, insertDataTypes.Length).ToArray();
			for (int i = 0; i < tableDataTypes.Length; i++)
			{

				if (tableDataTypes[i] == "int")
				{
					if (insertDataTypes[i].ToLower() == "system.int16" || insertDataTypes[i].ToLower() == "system.int32" || insertDataTypes[i].ToLower() == "system.int64")
					{
						test[i] = 1;
						continue;
					}

				}

				if (tableDataTypes[i] == "float")
				{
					if (insertDataTypes[i].ToLower() == "system.single" || insertDataTypes[i].ToLower() == "system.double") { test[i] = 1; continue; }
				}

				if (tableDataTypes[i] == "varchar")
				{
					if (insertDataTypes[i].ToLower() == "system.string")
					{
						test[i] = 1;
						continue;
					}
				}

				if (tableDataTypes[i] == "char")
				{
					if (insertDataTypes[i].ToLower() == "system.char")
					{
						test[i] = 1;
						continue;
					}
				}

				if (tableDataTypes[i] == "text")
				{

					if (insertDataTypes[i].ToLower() == "system.string")
					{
						TextCheacker(insertValues[i].ToString());
						test[i] = 1;
					}
				}

				if (tableDataTypes[i] == "datetime")
				{
					if (insertDataTypes[i].ToLower() == "system.datetime")
					{
						test[i] = 1;
						continue;
					}
				}

				if (test[i] == 0)
					throw new Exception($"Error: An invalid datatype is encounted\nexpecting type of '{tableDataTypes[i]}' instead of '{insertDataTypes[i]}'");

			}

		}
		
		/// <summary>
		/// to check if a number is equalto or greaterthan zero
		/// </summary>
		/// <param name="value">the number value in string type</param>
		/// <returns>true if it is a number and is equalto or greaterthan zero, else false </returns>
		internal bool IsPositive(string value)
		{
			if (CheckNum(value))
			{
				decimal a = decimal.Parse(value);
				return (a >= 0);
			}
			return false;
		}
		
		/// <summary>
		/// to check if a number lessthan zero
		/// </summary>
		/// <param name="value">the number value in string type</param>
		/// <returns>true if it is a number and is lessthan zero, else false </returns>
		internal bool IsNegative(string value)
		{
			if (CheckNum(value))
			{
				decimal a = decimal.Parse(value);
				return (a < 0);
			}
			return false;
		}

		internal DataTable HeadReader(out int tbRows, ZipArchiveEntry entryhead, Connection connection, Encryption encrypt)
		{
			ColumnDefination coldef = new ColumnDefination(); int tr = 0;
			using (StreamReader read = new StreamReader(entryhead.Open(), connection.CharSet))
			{
				int row = 0;
				while (!read.EndOfStream && row < 11)
				{
					string[] lines = encrypt.DecryptAll(read.ReadLine(),
									connection.Key,
									connection.IV,
									connection.CharSet);                 //column names

					int ind = 0;
					foreach (var data in lines)
					{
						
						if (row == 0)
						{
							coldef.AddColumn(new Isql.Collections.Column(data));
							ind++;
							continue;
						}
						else if (row == 1)
						{
							coldef[ind].DataType = Parser.TypeConverter(data);
							ind++;
							continue;
						}
						else if (row == 2)
						{
							coldef[ind].AcceptNull = (data.ToLower() == "null" || data.ToLower() == "true") ? true : false;
							ind++;
							continue;
						}
						else if (row == 3)
						{
							coldef[ind].Size = (data.ToLower() == "null" || data.ToLower() == "-1") ? -1 : Parser.DataConverter(typeof(int), data);
							ind++;
							continue;
						}
						else if (row == 4)
						{
							Type type = coldef[ind].DataType;
							if (data.ToLower() != "null")
								coldef[ind].DefaultValue = Default.Parse(data); 
							ind++;
							continue;
						}
						else if (row == 5)
						{
							coldef[ind].IsUnique = bool.Parse(data);
							ind++;
							continue;
						}
						else if (row == 6)
						{
							coldef[ind].Check = Check.Parse(data);
							ind++;
							continue;
						}
						else if (row == 7)
						{
							coldef[ind].ForeignKey = ForeignKey.Parse(data);
							ind++;
							continue;
						}
						else if (row == 8)
						{
							coldef[ind].Increament = AutoIncreament.Parse(data);
							ind++;
							continue;
						}
						else if (row == 9)
						{
							coldef[ind].PrimaryKey = PrimaryKey.Parse(data);
							ind++;
							continue;
						}

						else if (row == 10)
						{
							tr = int.Parse(data);
							break;
						}

						ind++;
					}
					row++;
				}
			}
			
			var dt = new DataTable();
			dt.Defined(coldef);

			dt.Name = entryhead.Name;
			tbRows = tr;
			return dt;
		}
		
		internal DataTable BodyReader(DataTable dt, ZipArchiveEntry entryBody, int tbRow, Connection connection, Encryption encrypt)
		{
			dt.IgnoreConstraints = true;
			using (StreamReader reader = new StreamReader(entryBody.Open(), connection.CharSet))
			{
				
				int row = 0;
				while (!reader.EndOfStream && row < tbRow)
				{
					string line = reader.ReadLine();
					
					string[] baseline = encrypt.DecryptAll(line, 
										connection.Key, 
										connection.IV,
										connection.CharSet);                 //column names
					
					Cell[] _cell = new Cell[dt.Head.Count];
					int id = 0;
					foreach (var b in baseline)
					{
						
						_cell[id] = new Cell(Parser.DataConverter(dt.Head[id].DataType, b.ToString()));
						id++;
						
					}
					
					dt.AddRow(new Row(_cell));
					row++;
				}
			}

			dt.IgnoreAutoIncreament = false;
			return dt;
		}
		
		internal void HeadWriter(DataTable dt, ZipArchiveEntry entryhead, Connection connection, Encryption encrypt)
		{
			
			using(BinaryWriter writer = new BinaryWriter(entryhead.Open(), connection.CharSet))
			{
				Detail detail = dt.GetDetail(dt.Head.NameList());

				byte[] name = encrypt.EncryptAll(detail.ColumnNames, 
												 connection.Key, connection.IV, 
												 connection.CharSet);

				byte[] type = encrypt.EncryptAll(Parser.StringTypeConverter(detail.DataTypes), 
												 connection.Key, connection.IV, 
												 connection.CharSet);

				byte[] nulls = encrypt.EncryptAll(Parser.ArrayToString(string.Join(",", detail.AcceptNull).Split(',')), 
												 connection.Key, connection.IV, 
												 connection.CharSet);
				

				byte[] size = encrypt.EncryptAll(Parser.ArrayToString(string.Join(",", detail.Size).Split(',')), 
												 connection.Key, connection.IV, 
												 connection.CharSet);

				byte[] def = encrypt.EncryptAll(Parser.ArrayToString(detail.DefaultValues), 
												 connection.Key, connection.IV, 
												 connection.CharSet);

				byte[] uni = encrypt.EncryptAll(Parser.ArrayToString(string.Join(",", detail.IsUnique).Split(',')),
												 connection.Key, connection.IV, 
												 connection.CharSet);

				byte[] che = encrypt.EncryptAll(Parser.ArrayToString(detail.Checks), 
												 connection.Key, connection.IV, 
												 connection.CharSet);

				byte[] forkey = encrypt.EncryptAll(Parser.ArrayToString(detail.ForeignKeys), 
												 connection.Key, connection.IV, 
												 connection.CharSet);

				byte[] inc = encrypt.EncryptAll(Parser.ArrayToString(detail.AutoIncreaments), 
												 connection.Key, connection.IV, 
												 connection.CharSet);

				byte[] prykey = encrypt.EncryptAll(Parser.ArrayToString(detail.PrimaryKeys), 
												 connection.Key, connection.IV, 
												 connection.CharSet);

				byte[] tbRow = encrypt.EncryptAll(new object[] { dt.Size()}, 
												 connection.Key, connection.IV, 
												 connection.CharSet);
				
				writer.Write(name, 0, name.Length);
				writer.Write(type, 0, type.Length);
				writer.Write(nulls, 0, nulls.Length);
				writer.Write(size, 0, size.Length);
				writer.Write(def, 0, def.Length);
				writer.Write(uni, 0, uni.Length);
				writer.Write(che, 0, che.Length);
				writer.Write(forkey, 0, forkey.Length);
				writer.Write(inc, 0, inc.Length);
				writer.Write(prykey, 0, prykey.Length);
				writer.Write(tbRow, 0, tbRow.Length);
			}
		}
		
		internal void BodyWriter(DataTable dt, ZipArchiveEntry entrybody, Connection connection, Encryption encrypt)
		{
			using(BinaryWriter writer = new BinaryWriter(entrybody.Open(), connection.CharSet))
			{
				foreach(var row in dt)
				{
					byte[] encRow = encrypt.EncryptAll(row.GetCurrentValues(),
														connection.Key, connection.IV,
														connection.CharSet);
					
					writer.Write(encRow, 0, encRow.Length); writer.Flush();
				}
			}
		}
		
		internal void BodyWriter(DataTable dt, ZipArchiveEntry entrybody, byte[] key, byte[] IV, Encryption encrypt, Encoding encoding)
		{
			using(BinaryWriter writer = new BinaryWriter(entrybody.Open(), encoding))
			{
				foreach(var row in dt)
				{
					byte[] encRow = encrypt.EncryptAll(row.GetCurrentValues(),
														key, IV, encoding);
					
					writer.Write(encRow, 0, encRow.Length);
				}
			}
		}
	}
}
