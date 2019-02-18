using SudokuLib.Model;
using Sudoku.ViewModel.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Sudoku.ViewModel.Converters
{
    public class CompletePuzzleNullOrPuzzleCompleteBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return (Puzzle)values[0] != null && (CompletionState)values[1] == CompletionState.Incomplete ? true : false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
