using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DT = Google.Documents.Document.DocumentType;

namespace Exult.Tasks.Google.Documents
{
    public enum ResourceType
    {
        document = DT.Document,
        spreadsheet = DT.Spreadsheet,
        pdf = DT.PDF,
        presentation = DT.Presentation,
        folder = DT.Folder,
        form = DT.Form,
        drawing = DT.Drawing,
        unknown = DT.Unknown,
        file = unknown + 1
    }
}
