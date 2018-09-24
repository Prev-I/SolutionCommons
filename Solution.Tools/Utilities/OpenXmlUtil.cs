using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;


namespace Solution.Tools.Utilities
{
    public static class OpenXmlUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /*  Sheet sheet = GetSheetFromWorkSheet(myWorkbookPart, myWorksheetPart);
            string sheetName = sheet.Name;
        */
        public static Sheet GetSheetFromWorkSheet(WorkbookPart workbookPart, WorksheetPart worksheetPart)
        {
            string relationshipId = workbookPart.GetIdOfPart(worksheetPart);
            IEnumerable<Sheet> sheets = workbookPart.Workbook.Sheets.Elements<Sheet>();
            return sheets.FirstOrDefault(s => s.Id.HasValue && s.Id.Value == relationshipId);
        }

        public static Worksheet GetWorkSheetFromSheet(WorkbookPart workbookPart, Sheet sheet)
        {
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
            return worksheetPart.Worksheet;
        }

        public static IEnumerable<KeyValuePair<string, Worksheet>> GetNamedWorksheets(WorkbookPart workbookPart)
        {
            return workbookPart.Workbook.Sheets.Elements<Sheet>()
                .Select(sheet => new KeyValuePair<string, Worksheet>
                    (sheet.Name, GetWorkSheetFromSheet(workbookPart, sheet)));
        }

        public static Sheet GetSheetFromName(WorkbookPart workbookPart, string sheetName)
        {
            return workbookPart.Workbook.Sheets.Elements<Sheet>().FirstOrDefault(s => s.Name.HasValue && s.Name.Value == sheetName);
        }

        public static WorksheetPart GetWorksheetPartFromName(WorkbookPart workbookPart, string sheetName)
        {
            Sheet sheet = GetSheetFromName(workbookPart, sheetName);
            if (sheet != null && sheet.Id != null)
                return (WorksheetPart)workbookPart.GetPartById(sheet.Id);
            else
                return workbookPart.AddNewPart<WorksheetPart>();
        }

    }
}
