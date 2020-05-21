///////////////////////////////////////////////////////////
//  ExcelCheckError.cs
//  Implementation of the Class ExcelCheckError
//  Generated by Enterprise Architect
//  Created on:      21-5��-2020 14:59:32
//  Original author: drago
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



namespace FD.Excel.Checker {
	public class ExcelCheckError {



		~ExcelCheckError(){

		}

		/// <summary>
		/// @param ="errorType"
		/// </summary>
		public ExcelCheckError(){

			this.ErrorList = new Dictionary<string, List<string>>();
		}

		/// <summary>
		/// @param ="errorType"
		/// </summary>
		/// <param name="errorList"></param>
		public ExcelCheckError(Dictionary<string, List<string>> errorList){

			this.ErrorList = ErrorList;
		}

		/// 
		/// <param name="key"></param>
		/// <param name="message"></param>
		public void AppendError(string key, string message){

			if (!ErrorList.ContainsKey(key))
			{
			    this.ErrorList.Add(key, new List<string>());
			}
			this.ErrorList[key].Add(message);
		}

		public string ErrorBrief{
			get;  set;
		}

		public string ErrorDetail{
			get;  set;
		}

		public Dictionary<string,List<string>> ErrorList{
			get;  set;
		}

	}//end ExcelCheckError

}//end namespace FD.Excel.Checker