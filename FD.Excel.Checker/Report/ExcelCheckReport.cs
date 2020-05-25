///////////////////////////////////////////////////////////
//  ExcelCheckReport.cs
//  Implementation of the Class ExcelCheckReport
//  Generated by Enterprise Architect
//  Created on:      22-5��-2020 8:56:59
//  Original author: drago
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;



namespace FD.Excel.Checker {
	public class ExcelCheckReport {

		public ExcelCheckReport(){

			this.CheckErrorList = new Dictionary<CheckErrorType, ExcelCheckError>();
		}

		public Dictionary<CheckErrorType,ExcelCheckError> CheckErrorList{
			get;  set;
		}

		/// 
		/// <param name="type"></param>
		public void Clear(CheckErrorType type){

			if (CheckErrorList.ContainsKey(type))
			    CheckErrorList[type].ErrorList.Clear();
		}

		/// 
		/// <param name="key"></param>
		/// <param name="message"></param>
		public void ColumnError(string key, string message){

			Error(CheckErrorType.ColumnCheck, key, message);
		}

		/// 
		/// <param name="type"></param>
		/// <param name="key"></param>
		/// <param name="message"></param>
		private void Error(CheckErrorType type, string key, string message){

			if (!CheckErrorList.ContainsKey(type))
			    CheckErrorList.Add(type, new ExcelCheckError());
			CheckErrorList[type].AppendError(key, message);
		}

		/// 
		/// <param name="key"></param>
		/// <param name="message"></param>
		public void ExcelReadError(string key, string message){

			Error(CheckErrorType.ExcelRead, key, message);
		}

		/// 
		/// <param name="key"></param>
		/// <param name="message"></param>
		public void ExcelStructureError(string key, string message){

			Error(CheckErrorType.ExcelStructure, key, message);
		}

		/// 
		/// <param name="key"></param>
		/// <param name="message"></param>
		public void FkError(string key, string message){

			Error(CheckErrorType.FKCheck, key, message);
		}

		/// 
		/// <param name="key"></param>
		/// <param name="message"></param>
		public void PkError(string key, string message){

			Error(CheckErrorType.PKCheck, key, message);
		}

		/// 
		/// <param name="key"></param>
		/// <param name="message"></param>
		public void TemplateMatchError(string key, string message){

			Error(CheckErrorType.TemplateMatch, key, message);
		}
		 

		public string Report()
		{
			var sb = new StringBuilder();
			foreach (var error in CheckErrorList)
			{
				Enum
				sb.Append(EnumHelper.GetEnumDescription(error.Key));
				sb.AppendLine("------");
				sb.Append(error.Value.ErrorDetailed);
				sb.AppendLine("------");
			}
			return sb.ToString();
		}

	}//end ExcelCheckReport

}//end namespace FD.Excel.Checker