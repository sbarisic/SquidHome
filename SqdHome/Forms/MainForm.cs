using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SqdHome {
	public partial class MainForm : Form {
		public MainForm() {
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e) {
			int ColCount = 4;
			float Percent = 100 / ColCount;

			tableLayout.ColumnCount = ColCount;
			for (int i = 0; i < ColCount; i++)
				tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, Percent));

			tableLayout.RowCount = 0;
			tableLayout.RowStyles.Clear();

			int RowNum = AddRow();
			tableLayout.Controls.Add(new Label() { Text = "ID" }, 0, RowNum);
			tableLayout.Controls.Add(new Label() { Text = "Name" }, 1, RowNum);
			tableLayout.Controls.Add(new Label() { Text = "Value" }, 2, RowNum);
			tableLayout.Controls.Add(new Label() { Text = "Action" }, 3, RowNum);
		}

		int AddRow() {
			tableLayout.RowCount++;
			tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

			return tableLayout.RowCount - 1;
		}
	}
}
