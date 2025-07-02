using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using OfficeTicketingTool.Models.Enums;

namespace OfficeTicketingTool.Converters
{
      
        public class StatusColorConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is TicketStatus status)
                {
                    return status switch
                    {
                        TicketStatus.Open => Brushes.Blue,
                        TicketStatus.InProgress => Brushes.Orange,
                        TicketStatus.OnHold => Brushes.Gray,
                        TicketStatus.Resolved => Brushes.Green,
                        TicketStatus.Closed => Brushes.Red,
                        _ => Brushes.Black
                    };
                }
                return Brushes.Black;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }


    }


   
