///////////////////////////////////////////////////////////
//  ExcelChecker.cs
//  Implementation of the Class ExcelChecker
//  Generated by Enterprise Architect
//  Created on:      21-5��-2020 16:57:37
//  Original author: drago
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;



using NPOI.SS.UserModel;
using System.Data;
using FD.Excel.Checker;
namespace FD.Excel.Checker {
	public class ExcelChecker {

		private ExcelStructure _excelStructure;
		/// <summary>
		/// { "date", "datetime" };
		/// </summary>
		private readonly string [] _fromData = { "date" };
		/// <summary>
		/// number
		/// </summary>
		private readonly string [] _fromDouble = { "number" };
		/// <summary>
		/// integer
		/// </summary>
		private readonly string [] _fromInt = { "integer" };
		/// <summary>
		/// multi_dic
		/// </summary>
		private readonly string [] _fromMultiDic = { "multi_dic" };
		/// <summary>
		/// integer,number
		/// </summary>
		private readonly string [] _fromNumeric = { "integer", "number" };
		/// <summary>
		/// single_dic
		/// </summary>
		private readonly string [] _fromSingleDic = { "single_dic" };
		/// <summary>
		/// nvarchar2,varchar2
		/// </summary>
		private readonly string [] _fromString = { "nvarchar2", "varchar2" };
		private IList<TemplateRule> _templateRules;
		private IWorkbook _workbook;
		private const string NullHeader = "NullHeader";

		public ExcelChecker(){

			//��ʼ����鱨��
			_checkReport = new ExcelCheckReport();
		}

		/// 
		/// <param name="stream"></param>
		public ExcelChecker(Stream stream): this(){

			try
			{
			    _workbook = WorkbookFactory.Create(stream);
			}
			catch (Exception ex)
			{
			    var message = "---��ȡExcel�ļ�ʱ����δ֪�������ȷ���ļ���Ч����δ�����������򿪣�����ϵϵͳ����Ա!\r\n" + ex.Message;
			    _checkReport.ExcelReadError("�ļ���ȡʧ��", message);
			    _workbook = null;
			}
		}

		~ExcelChecker(){

		}

		public ExcelCheckReport _checkReport{
			get; set;
		}

		private DataSet _dataSet{
			get; set;
		}

		public TemplateRule _templateRule{
			get; set;
		}

		/// 
		/// <param name="errorList"></param>
		/// <param name="key"></param>
		/// <param name="message"></param>
		private void AppendError(Dictionary<string, List<string>> errorList, string key, string message){

			if (!errorList.Keys.Contains(key))
			    errorList.Add(key, new List<string>());
			errorList[key].Add(message);
		}

		private void CheckDataSet(){

			#region ---ѭ������Table
			foreach (DataTable aTable in _dataSet.Tables)
			{
			    var aTableRowList = aTable.Select().ToList();
			    var aSheetRule = _templateRule[aTable.TableName];
			    var aSheetStructure = _excelStructure[aTable.TableName];
		
			    var checkNullColName = new List<string>();
		
			    #region �м��
			    foreach (var aColumnStructure in aSheetStructure.Columns)
			    {
			        if (aColumnStructure == null || aColumnStructure.ColumnName.Contains(NullHeader))
			            continue;
		
			        var aColumnRule = aSheetRule[aColumnStructure.ColumnName];
			        if (aColumnRule == null)
			            continue;
			        List<string> columnValues;
			        int iRowNum;
			        var ErrorKey = $"[{aSheetStructure.SheetName}]��.[{aColumnStructure.ColumnName}]��";
			        #region //���ɿؼ��
			        if (!aColumnRule.IsNullable)
			        {
			            iRowNum = aTable.Select(String.Format("[{0}] is null ", aColumnStructure.ColumnName)).Count();
			            if (iRowNum > 0)
			            {
			                _checkReport.ColumnError(ErrorKey, $"----[{"����Ϊ�ռ��"}]��ͨ��������Ϊ�����ֶΣ���{iRowNum}ֵΪ�գ�");
			            }
			        }
			        #endregion
		
			        #region //"�ַ���" aColumnRule.MeanDataType =="�ַ���"
			        if (aColumnRule.MeanDataType.ToLower() == "nvarchar2")
			        {
			            iRowNum = aTable.Select(String.Format("len([{0}]) > {1} ", aColumnRule.ColumnName, aColumnRule.FieldLength)).Count();
			            if (iRowNum > 0)
			            {
			                _checkReport.ColumnError(ErrorKey, $"----[{"�ַ������ȼ��"}]��ͨ�����������ݳ���Ӧ��С�ڵ���{aColumnRule.FieldLength}������{iRowNum}ֵ���ȳ���{aColumnRule.FieldLength}��");
			            }
			        }
		
			        if (aColumnRule.MeanDataType.ToLower() == "nvarchar2")
			        {
			            iRowNum = aTable.Select(String.Format("len([{0}]) > {1} ", aColumnRule.ColumnName, aColumnRule.FieldLength)).Count();
			            if (iRowNum > 0)
			            {
			                _checkReport.ColumnError(ErrorKey, $"----[{"�ַ������ȼ��"}]��ͨ�����������ݳ���Ӧ��С�ڵ���{aColumnRule.FieldLength}������{iRowNum}ֵ���ȳ���{aColumnRule.FieldLength}��");
			            }
			        }
			        #endregion
		
			        #region //"���ֵ���"
			        if (_fromSingleDic.Contains(aColumnRule.MeanDataType.ToLower()))
			        {
			            columnValues = aTable.Select(String.Format("[{0}] is not null ", aColumnStructure.ColumnName)).ToList()
			                            .Select(p => p[aColumnStructure.ColumnIndex].ToString())
			                            .Distinct().ToList();
			            foreach (var aParName in columnValues)
			            {
			                if (aColumnRule.DictList != null && aColumnRule.DictList.ContainsKey(aParName))
			                {
			                    foreach (var aRow in aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aParName)))
			                    {
			                        aRow[aColumnRule.ColumnName] = aColumnRule.DictList[aParName].ToString();
			                    }
			                }
			                else
			                {
			                    iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aParName)).Count();
			                    string checkFromDict = string.Empty;
			                    if (aColumnRule.DictList != null)
			                    {
			                        foreach (var aDict in aColumnRule.DictList)
			                        {
			                            if (string.IsNullOrEmpty(checkFromDict))
			                                checkFromDict = "----[ע]��ѡ������Ϊ��" + "��" + aDict.Key + "��";
			                            else
			                                checkFromDict = checkFromDict + "," + "��" + aDict.Key + "��";
			                        }
			                        checkFromDict += ";";
			                    }
			                    else
			                    {
			                        checkFromDict = "----ϵͳ��û��Ϊ����˾���á�" + aColumnRule.ColumnName + "���Ŀ�ѡ���ݣ�����ϵϵͳ����Ա��";
			                    }
			                    _checkReport.ColumnError(ErrorKey, checkFromDict);
		
			                    _checkReport.ColumnError(ErrorKey, $"----[�ֵ���]��ͨ������{aColumnRule.ColumnName}����Ӧ�ֵ���������⣬����{iRowNum}Ϊ��ֵ������ϵ����Ա��");
			                }
		
			            }
		
			        }
			        #endregion
		
			        #region //"���ֵ���"
			        if (_fromMultiDic.Contains(aColumnRule.MeanDataType.ToLower()))
			        {
			            columnValues = aTable.Select(String.Format("[{0}] is not null ", aColumnStructure.ColumnName)).ToList()
			                            .Select(p => p[aColumnStructure.ColumnIndex].ToString())
			                            .Distinct().ToList();
			            foreach (var aParNameStr in columnValues)
			            {
			                iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aParNameStr)).Count();
			                var aParNameValue = string.Empty;
			                aParNameValue = aParNameStr.Replace("��", ",");
			                aParNameValue = aParNameStr.Replace("��", ",");
			                var aParNameList = aParNameStr.Split(',').Distinct().ToList();
			                aParNameList.RemoveAll(f => string.IsNullOrEmpty(f.Trim()));
			                if (aParNameList != null)
			                {
			                    foreach (var aParName in aParNameList)
			                    {
			                        if (aColumnRule.DictList != null && aColumnRule.DictList.ContainsKey(aParName))
			                        {
			                            if (aParNameValue == null)
			                                aParNameValue = aColumnRule.DictList[aParName].ToString();
			                            else
			                                aParNameValue = aParNameValue + "," + aColumnRule.DictList[aParName];
			                        }
			                        else
			                        {
			                            iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aParName)).Count();
			                            string checkFromDict = string.Empty;
			                            if (aColumnRule.DictList != null)
			                            {
			                                foreach (var aDict in aColumnRule.DictList)
			                                {
			                                    if (string.IsNullOrEmpty(checkFromDict))
			                                        checkFromDict = "----[ע]��ѡ������Ϊ��" + "��" + aDict.Key + "��";
			                                    else
			                                        checkFromDict = checkFromDict + "," + "��" + aDict.Key + "��";
			                                }
			                                checkFromDict += ";";
			                            }
			                            else
			                            {
			                                checkFromDict = "----ϵͳ��û��Ϊ����˾���á�" + aColumnRule.ColumnName + "���Ŀ�ѡ���ݣ�����ϵϵͳ����Ա��";
			                            }
			                            _checkReport.ColumnError(ErrorKey, checkFromDict);
		
			                            _checkReport.ColumnError(ErrorKey, $"----[�ֵ���]��ͨ������{aColumnRule.ColumnName}����Ӧ�ֵ���������⣬����{iRowNum}Ϊ��ֵ������ϵ����Ա��");
			                        }
			                    }
			                }
			                if (aParNameValue != null)
			                {
			                    foreach (var aRow in aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aParNameStr)))
			                    {
			                        aRow[aColumnRule.ColumnName] = aParNameValue;
			                    }
			                }
			            }
			        }
			        #endregion
		
			        #region //"С����":"ʮ������":Ĭ����λС��
			        if (_fromDouble.Contains(aColumnRule.MeanDataType.ToLower()))
			        {
			            columnValues = aTable.Select(String.Format("[{0}] is not null ", aColumnStructure.ColumnName)).ToList()
			                                                  .Select(p => p[aColumnStructure.ColumnIndex].ToString())
			                                                  .Distinct().ToList();
			            foreach (var aValue in columnValues)
			            {
			                decimal? aDecimalValue = null;
			                if (aValue.EndsWith("%"))
			                {
			                    var aStrValue = aValue.TrimEnd('%');
			                    aDecimalValue = decimal.TryParse(aStrValue, out var tmpVal) ? tmpVal / 100 : (decimal?)null;
			                }
			                else
			                {
			                    aDecimalValue = decimal.TryParse(aValue, out var tmpVal) ? tmpVal : (decimal?)null;
			                }
		
			                if (aDecimalValue.HasValue && aColumnRule.FieldSacle > 0)
			                {
			                    aDecimalValue = Math.Round(aDecimalValue.Value, aColumnRule.FieldSacle);
			                }
		
			                if (!aDecimalValue.HasValue)
			                {
			                    iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)).Count();
			                    _checkReport.ColumnError(ErrorKey, $"----[С���ͼ��]��ͨ������{aValue}������ת��Ϊ{aColumnRule.FieldSacle}λС��������{iRowNum}��Ϊ��ֵ��");
			                }
			                else if (aColumnRule.DecimalMax.HasValue && aDecimalValue < aColumnRule.DecimalMax)
			                {
			                    iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)).Count();
			                    _checkReport.ColumnError(ErrorKey, $"----[С����Χ���]��ͨ������{aValue}������С��{aColumnRule.DecimalMax}������{iRowNum}��Ϊ��ֵ��");
			                }
			                else if (aColumnRule.DecimalMin.HasValue && aDecimalValue < aColumnRule.DecimalMin)
			                {
			                    iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)).Count();
			                    _checkReport.ColumnError(ErrorKey, $"----[С����Χ���]��ͨ������{aValue}���������{aColumnRule.DecimalMin}������{iRowNum}��Ϊ��ֵ��");
			                }
			                else
			                {
			                    if (aValue.EndsWith("%")) //����ΪС��ֵ
			                    {
			                        foreach (var aRow in aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)))
			                        {
			                            aRow[aColumnRule.ColumnName] = aDecimalValue.Value;
			                        }
			                    }
			                }
		
		
			            }
			        }
			        #endregion
		
			        #region //"������"
			        if (_fromInt.Contains(aColumnRule.MeanDataType.ToLower()))
			        {
			            columnValues = aTable.Select(String.Format("[{0}] is not null ", aColumnStructure.ColumnName)).ToList()
			                                                  .Select(p => p[aColumnStructure.ColumnIndex].ToString())
			                                                  .Distinct().ToList();
			            foreach (var aValue in columnValues)
			            {
			                int? aIntValue = null;
			                aIntValue = Int32.TryParse(aValue, out var tmpVal) ? tmpVal : (int?)null;
			                if (!aIntValue.HasValue)
			                {
			                    iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)).Count();
			                    _checkReport.ColumnError(ErrorKey, $"----[�����ͼ��]��ͨ������{aValue}������ת��Ϊ����������{iRowNum}��Ϊ��ֵ��");
			                }
			                else if (aColumnRule.DecimalMax.HasValue && aIntValue < aColumnRule.DecimalMax)
			                {
			                    iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)).Count();
			                    _checkReport.ColumnError(ErrorKey, $"----[������Χ���]��ͨ������{aValue}������С��{aColumnRule.DecimalMax}������{iRowNum}��Ϊ��ֵ��");
			                }
			                else if (aColumnRule.DecimalMin.HasValue && aIntValue < aColumnRule.DecimalMin)
			                {
			                    iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)).Count();
			                    _checkReport.ColumnError(ErrorKey, $"----[������Χ���]��ͨ������{aValue}���������{aColumnRule.DecimalMin}������{iRowNum}��Ϊ��ֵ��");
			                }
		
		
		
			            }
			        }
			        #endregion
		
			        #region //"������"
			        if (aColumnRule.MeanDataType.ToLower()=="date")
			        {
			            columnValues = aTable.Select(String.Format("[{0}] is not null ",
			                aColumnStructure.ColumnName)).ToList()
			                           .Select(p => p[aColumnStructure.ColumnIndex].ToString())
			                           .Distinct().ToList();
			            foreach (var aValue in columnValues)
			            {
			                DateTime? aDateTime = null;
			                aDateTime = DateTime.TryParse(aValue, out var tmpVal) ? tmpVal : (DateTime?)null;
			                if (!aDateTime.HasValue)
			                {
			                    iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)).Count();
			                    _checkReport.ColumnError(ErrorKey, $"----[�����ͼ��]��ͨ������{aValue}������ת��Ϊ���ڣ�����{iRowNum}��Ϊ��ֵ��");
			                }
			                else if (aColumnRule.DateTimeMax.HasValue && aDateTime>aColumnRule.DateTimeMax)
			                {
			                    iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)).Count();
			                    _checkReport.ColumnError(ErrorKey, $"----[���ڷ�Χ���]��ͨ������{aValue}������С��{aColumnRule.DateTimeMax}������{iRowNum}��Ϊ��ֵ��");
			                }
			                else if (aColumnRule.DateTimeMin.HasValue&& aDateTime<aColumnRule.DateTimeMin)
			                {
			                    iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)).Count();
			                    _checkReport.ColumnError(ErrorKey, $"----[���ڷ�Χ���]��ͨ������{aValue}���������{aColumnRule.DateTimeMin}������{iRowNum}��Ϊ��ֵ��");
			                }
			            }
		
			        }
			        #endregion
		
		
			    }
			    #endregion //�м��
		
			    var pkList = aSheetRule.PKColumnNameList;
			    var fkList = aSheetRule.FKColumnNameList;
		
			    var pkValueList = new Dictionary<string, int>();
			    var fkValueListSelf = new Dictionary<string, int>();
			    var fkValueListFather = new List<string>();
		
			    #region //�������
			    if (pkList.Any())
			    {
			        foreach (var aRow in aTableRowList)
			        {
			            string aPkValue = string.Empty;
			            foreach (var aPk in pkList)
			            {
			                aPkValue = aPkValue + string.Format("[{0}]=��{1}��;", aPk, aRow[aPk]);
			            }
			            if (!pkValueList.Keys.Contains(aPkValue))
			            {
			                pkValueList.Add(aPkValue, 1);
			            }
			            else
			            {
			                pkValueList[aPkValue] = pkValueList[aPkValue] + 1;
			            }
		
			            if(!string.IsNullOrEmpty(aSheetRule.FkSheetName) && fkList.Any())
			            {
			                string aFkValue = string.Empty;
			                foreach (var aFk in fkList.Keys)
			                {
			                    aFkValue = aFkValue+ string.Format("[{0}]=��{1}��;", fkList[aFk], aRow[aFk]);
			                }
			                if (!fkValueListSelf.Keys.Contains(aFkValue))
			                {
			                    fkValueListSelf.Add(aFkValue, 1);
			                }
			                else
			                {
			                    fkValueListSelf[aFkValue] = fkValueListSelf[aFkValue] + 1;
			                }
			            }
			        }
		
			        foreach (var aPkError in pkValueList.Where(pk=>pk.Value>1)
			            .Select(pk=>pk.Key).ToList())
			        {
			            _checkReport.PkError(aSheetStructure.SheetName, $"----[Ψһ�Լ��]��ͨ����{aPkError}��Ӧ{pkValueList[aPkError]}����¼��");
			        }
			    }
			    #endregion //�������
		
			    #region //������
			    if(fkList.Any() && fkValueListSelf.Count > 0)
			    {
			        var fDataTable = _dataSet.Tables[aSheetRule.FkSheetName];
			        foreach (DataRow aRow in fDataTable.Rows)
			        {
			            string aFkValue = string.Empty;
			            foreach (var aFk in fkList.Keys)
			            {
			                aFkValue = aFkValue + string.Format("[{0}]=��{1}��;", fkList[aFk], aRow[fkList[aFk]]);
			            }
			            if (!fkValueListFather.Contains(aFkValue))
			            {
			                fkValueListFather.Add(aFkValue);
			                fkValueListSelf.Remove(aFkValue);
			            }
			            if (fkValueListSelf.Any())
			            {
			                foreach (var aFkError in fkValueListSelf.Keys)
			                {
			                    _checkReport.FkError(aSheetStructure.SheetName, $"----[������]��ͨ����{aFkError}�ڹ�����[{aSheetRule.FkSheetName}]��û���ҵ���Ӧ��Ϣ������{fkValueListSelf[aFkError]}��Ϊ�������");
			                }                        
			            }
			        }
			    }
			    #endregion
			}
			#endregion
		}

		/// 
		/// <param name="propertyList"></param>
		/// <param name="delimiter"></param>
		private string ConvertListToString(List<string> propertyList, string delimiter = "$$$"){

			string result = null;
			if (propertyList != null && propertyList.Any())
			{
			    foreach (var a in propertyList)
			    {
			        if (!string.IsNullOrEmpty(a))
			            result = result + a + delimiter;
			    }
			}
			if (result != null)
			{
			    result = result.Substring(0, result.Length - delimiter.Length);
			}
			return result;
		}

		private List<string> GetExcelTablesName(){

			var list = new List<string>();
			foreach (ISheet sheet in _workbook)
			{
			    list.Add(sheet.SheetName);
			}
			return list;
		}

		private void InitDatSet(){

			if (_dataSet == null)
			    _dataSet = new DataSet();
			foreach (var aSheetRule in _templateRule.SheetRules)
			{
			    if (aSheetRule.ColumnNum > 0)
			    {
			        _dataSet.Tables.Add(InitDatTable(aSheetRule));
			    }
			}
		}

		/// 
		/// <param name="aSheetRule"></param>
		private DataTable InitDatTable(SheetRule aSheetRule){

			var aSheetStructure = _excelStructure[aSheetRule.SheetName];
			var sheet = _workbook.GetSheetAt(aSheetRule.SheetIndex);
			var table = new DataTable(aSheetRule.SheetName);
			foreach (var aColumn in aSheetStructure.Columns)
			{
			    table.Columns.Add(aColumn.ColumnName);
			}
		
			var iRowNo = 0;
			var continueNullRowNum = 0; //�����ϵ3��Ϊ��,�������������ݼ��
		
			var rowEnumerator = sheet.GetRowEnumerator();
			rowEnumerator.MoveNext();//������һ��������
			while (rowEnumerator.MoveNext())
			{
			    iRowNo++;
			    var iNullColumnNum = 0;
			    var current = (IRow)rowEnumerator.Current;
			    var rowNew = table.NewRow();
			    foreach (var aColumnStructure in aSheetStructure.Columns)
			    {
			        if (aColumnStructure.ColumnName.Contains(NullHeader))
			        {
			            iNullColumnNum++;
			            continue;
			        }
			        var aColumnRule = aSheetRule[aColumnStructure.ColumnName];
			        if (aColumnRule == null)
			        {
			            iNullColumnNum++;
			            continue;
			        }
		
			        string value = null;
			        var cell = current.GetCell(aColumnStructure.ColumnIndex);
			        var cellTypeText = string.Empty;
			        try
			        {
			            if (cell != null && cell.ToString().Trim() != string.Empty)
			            {
			                switch (cell.CellType)
			                {
			                    case CellType.String:
			                        value = cell.StringCellValue.Trim();
			                        break;
			                    case CellType.Formula:
			                        switch (cell.CachedFormulaResultType)
			                        {
			                            case CellType.String:
			                                value = cell.StringCellValue.Trim();
			                                break;
			                            case CellType.Numeric:
			                                if (DateUtil.IsCellDateFormatted(cell))
			                                {
			                                    cellTypeText = "����";
			                                    value = cell.DateCellValue.ToString();
			                                }
			                                else
			                                {
			                                    cellTypeText = "����";
			                                    value = cell.NumericCellValue.ToString();
			                                }
			                                break;
			                            default:
			                                value = cell.ToString().Trim();
			                                break;
			                        }
		
			                        break;
			                    case CellType.Numeric:
			                        if (DateUtil.IsCellDateFormatted(cell))
			                        {
			                            value = cell.DateCellValue.ToString();
			                            break;
			                        }
			                        // �����Զ���ʱ���ʽ�����޷��ж�
			                        value = cell.ToString().Trim();
			                        break;
			                    default:
			                        value = cell.ToString().Trim();
			                        break;
			                }
			            }
			        }
			        catch (Exception ex) //3.1�������ͼ��
			        {
			            var message = $"----[��ֵʧ��]�����Զ�ȡ��{iRowNo}�� ��{aColumnStructure.ColumnName}����ֵʧ�ܣ����ֶ����õ�Ԫ���ʽΪ��{cellTypeText}��!";
			            _checkReport.ColumnError($"[{aSheetStructure.SheetName}]��.[{aColumnStructure.ColumnName}]��", message);
			        }
			        if (value == null)
			        {
			            iNullColumnNum++;
			        }
			        rowNew[aColumnStructure.ColumnIndex] = value;
			    }
			    if (iNullColumnNum >= aSheetStructure.ColumnNum)//ȫ��Ϊ��
			    {
			        iRowNo--;
			        continueNullRowNum++;
			        if (continueNullRowNum >= 2) //ȫ�ճ���3��,��������ȡ
			        {
			            break;
			        }
			        continue;
			    }
			    continueNullRowNum = 0;
			    table.Rows.Add(rowNew);
			}
			return table;
		}

		/// <summary>
		/// ��ȡExcel�ĵ��ṹ
		/// </summary>
		private void InitExcelStructure(){

			try
			{
			    _excelStructure = new ExcelStructure();
			    var sheets = _excelStructure.Sheets;
		
			    var excelTablesName = GetExcelTablesName(); //��ȡexcel��ͷ
			    if (excelTablesName == null || !excelTablesName.Any()) //1.0���޹�����
			    {
			        _checkReport.ExcelStructureError("�޹�����", "û���ҵ�������������Excel�Ƿ���Ч!");
		
			    }
			    else
			    {
			        var sheetIndex = -1;
			        foreach (var sheetName in excelTablesName)
			        {
			            sheetIndex++;
			            if (string.IsNullOrWhiteSpace(sheetName))
			                _checkReport.ExcelStructureError("������������Ч", $"----��{sheetIndex + 1}��������������Ч!");
			            else
			            {
			                var aSheet = new ExcelSheet()
			                {
			                    SheetIndex = sheetIndex,
			                    SheetName = sheetName
			                };
			                var columns = aSheet.Columns;
			                var sheet = _workbook.GetSheet(sheetName);
			                var headerrow = sheet.GetRow(0);
			                if (headerrow == null)
			                {
			                    _checkReport.ExcelStructureError("���ڿչ�����", $"----��{sheetIndex + 1}��������,Ϊ�չ�����!");
			                }
			                else
			                {
			                    var lastCellNum = headerrow.LastCellNum;
			                    var continueNullHeader = 0;
			                    for (int colIndex = 0; colIndex < lastCellNum; colIndex++)
			                    {
			                        try
			                        {
			                            var cell = headerrow.GetCell(colIndex);
			                            if (cell == null || cell.ToString().Trim() == "") //1.3 ����Ϊ��������
			                            {
			                                //aCol.ColumnName = NullHeader + colIndex;
			                                //cols.Add(aCol); ����ͷ�����뵽���ṹ��
			                                continueNullHeader++;
			                                if (continueNullHeader > 2) break;
			                            }
			                            else
			                            {
			                                continueNullHeader = 0;
			                                var value = cell.CellType == CellType.String ?
			                                    cell.StringCellValue.Trim() : cell.ToString().Trim();
			                                if (columns.Any(f => f.ColumnName == Convert.ToString(value))) //1.4�����ظ�����
			                                {
			                                    _checkReport.ExcelStructureError("�������ظ�", $"----[{sheetName}]������,���ڶ������Ϊ[{value}]����!");
			                                }
			                                else
			                                {
			                                    var aCol = new ExcelColumn()
			                                    {
			                                        ColumnIndex = colIndex,
			                                        ColumnName = value,
			                                    };
			                                    columns.Add(aCol);
			                                }
			                            }
			                        }
			                        catch (Exception ex)
			                        {
			                            _checkReport.ExcelStructureError("�����ƶ�ȡʧ��", $"----[{sheetName}]������,��{colIndex + 1},�����ƶ�ȡʧ��!");
			                        }
			                    }
			                }
			                sheets.Add(aSheet);
			            }
			        }
		
			    }
			}
			catch (Exception ex)
			{
			    _checkReport.ExcelStructureError("��ȡ���ṹ���ִ���", $"----[������Ϣ]{ex.Message}");
			}
		}

		/// <summary>
		/// ����ģ���б�,ƥ��ģ��
		/// </summary>
		private void MatchExcelTemplate(){

			foreach (var aTemplateRule in _templateRules)
			{
			    if (MatchTemplate(aTemplateRule)) //ƥ�䵽��һ��ok��ģ��
			    {
			        _templateRule = aTemplateRule;
			        //ƥ��ɹ����������
			        _checkReport.Clear(CheckErrorType.TemplateMatch);
			        break;
			    }
			}
		}

		/// <summary>
		/// ƥ��Excelģ��
		/// </summary>
		/// <param name="matchErrorList"></param>
		/// <param name="aTemplateRule"></param>
		private bool MatchTemplate(TemplateRule aTemplateRule){

			if (aTemplateRule.SheetNum == _excelStructure.SheetNum)
			{
			    foreach (var aSheet in _excelStructure.Sheets)
			    {
			        if (aTemplateRule.SheetRules.Any(s => s.SheetName == aSheet.SheetName))
			        {
			            var unMatchColumns = aTemplateRule[aSheet.SheetName].ColumnRules
			                .Where(c => !aSheet.Columns.Any(a => a.ColumnName == c.ColumnName))
			                .Select(c => c.ColumnName)
			                .ToList();
			            if (!unMatchColumns.Any())
			                continue;
			            var message = $"----������[{aSheet.SheetName}]��������:[{ConvertListToString(unMatchColumns, "]��[")}]��ģ�塶{aTemplateRule.TemplateName}��.[{aSheet.SheetName}]����ƥ�䣡";
			            _checkReport.TemplateMatchError("ƥ����ʧ��", message);
			            return false;
			        }
			        else //2.1.2 ģ�治����Excel�е�ĳһ��Sheetҳ,����Sheetҳ��������һ��
			        {
			            var message = $"----ģ�塶{aTemplateRule.TemplateName}����û���ҵ���[{aSheet.SheetName}](��{aSheet.ColumnNum}��)ƥ��Ĺ���������ƥ��!";
			            _checkReport.TemplateMatchError("ƥ�乤����ʧ��", message);
			            return false;
			        }
			    }
			}
			else
			{
			    var message = $"----ģ��<<{aTemplateRule.TemplateName}>>��{aTemplateRule.SheetNum}��������;��Excel����{_excelStructure.SheetNum}��������,��ƥ��!";
			    _checkReport.TemplateMatchError("ƥ�乤����ҳ��ʧ��", message);
			    return false;
			}
			return true;
		}

		public void StartCheck(){

		}

	}//end ExcelChecker

}//end namespace FD.Excel.Checker