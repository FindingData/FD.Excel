///////////////////////////////////////////////////////////
//  SheetRule.cs
//  Implementation of the Class SheetRule
//  Generated by Enterprise Architect
//  Created on:      21-5��-2020 16:57:06
//  Original author: drago
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;



using FD.Excel.Checker;
namespace FD.Excel.Checker {
	public class SheetRule {

		public SheetRule(){

		}

		public int ColumnNum{
			get
						            {
						                return ColumnRules?.Count() ?? 0;
						            }
		}

		public IList<ColumnRule> ColumnRules{
			get;  set;
		}

		/// <summary>
		/// ���ؽ�ֵ�ԡ�key�����ColumnName,Value����Sheet�ж�Ӧ������
		/// </summary>
		public Dictionary<string, string> FKColumnNameList{
			get
						            {
						                return ColumnRules.Where(f => f.IsFk).ToDictionary(k => k.ColumnName, v => v.FKColumnName);
						            }
		}

		public string FkSheetName{
			get;  set;
		}

		public List<string> PKColumnNameList{
			get
						            {
						                return ColumnRules.Where(f => f.IsPk).Select(f => f.ColumnName).ToList();
						            }
		}

		public int SheetIndex{
			get;  set;
		}

		public string SheetName{
			get;  set;
		}

		/// 
		/// <param name="index"></param>
		public ColumnRule this[int index]{
			get{return ColumnRules?[index];}
		}

		/// 
		/// <param name="name"></param>
		public ColumnRule this[string name]{
			get{return ColumnRules?.FirstOrDefault(f => name.Equals(f.ColumnName));}
		}

	}//end SheetRule

}//end namespace FD.Excel.Checker