﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using MemorizeIt.MemorySourceSupplier;
using MemorizeIt.Model;

namespace GoogleMemorySupplier
{
    public class PublicGoogleMemorySourceSupplier : IMemorySourceSupplier
    {
        private readonly string applicationName = "MemorizeIt";
        private readonly string spreadsheetKey;
        private readonly string userName;
        private readonly string password;

        public PublicGoogleMemorySourceSupplier(string spreadsheetKey, string userName, string password)
        {
            this.spreadsheetKey = spreadsheetKey;
            this.userName = userName;
            this.password = password;
        }

        public MemoryTable Download(string sheetName)
        {
            SpreadsheetsService service = new SpreadsheetsService(applicationName);
            service.setUserCredentials(userName, password);
            WorksheetEntry worksheet = this.GetWorksheetEntrees(service).FirstOrDefault(e => e.Title.Text == sheetName);
            if (worksheet == null)
                return null;

            CellQuery cellQuery = new CellQuery(worksheet.CellFeedLink);
            cellQuery.MaximumColumn = 2;

            CellFeed cellFeed = service.Query(cellQuery);
            List<MemoryItem> retval = new List<MemoryItem>();
            for (int i = 0; i < cellFeed.Entries.Count; i = i + 2)
            {
                retval.Add(
                    new MemoryItem(new string[] { ((CellEntry)cellFeed.Entries[i]).Value, ((CellEntry)cellFeed.Entries[i + 1]).Value }));
            }
            return new MemoryTable(sheetName, retval.ToArray());
        
        }

        public IEnumerable<string> GetSourcesList()
        {
            var retval = new List<string>();

            // Iterate through each worksheet in the spreadsheet.
            foreach (WorksheetEntry entry in this.GetWorksheetEntrees())
            {
                retval.Add(entry.Title.Text);
            }

            return retval;
        }

        public void CreateTemplate()
        {
            throw new InvalidOperationException("template creation is not avalible for public sources");
        }

        private IEnumerable<WorksheetEntry> GetWorksheetEntrees(SpreadsheetsService service = null)
        {
            if (service == null)
                service = new SpreadsheetsService(applicationName);
            service.setUserCredentials(userName, password);

            var worksheetQuery = new WorksheetQuery(spreadsheetKey, "private", "full");

            var wsFeed = service.Query(worksheetQuery);
            return wsFeed.Entries.OfType<WorksheetEntry>();
        }
    }
}
