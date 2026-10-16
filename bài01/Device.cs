using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeviceManager;

// Implement INotifyPropertyChanged để khi sửa thuộc tính, DataGridView tự cập nhật
public class Device : INotifyPropertyChanged
{
    private string _id;
    private string _name;
    private string _category;
    private decimal _price;
    private int _quantity;

    public string Id        { get => _id;       set => Set(ref _id, value); }
    public string Name      { get => _name;     set => Set(ref _name, value); }
    public string Category  { get => _category; set => Set(ref _category, value); }
    public decimal Price    { get => _price;    set => Set(ref _price, value); }
    public int Quantity     { get => _quantity; set => Set(ref _quantity, value); }

    public event PropertyChangedEventHandler PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string prop = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
