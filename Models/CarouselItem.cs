namespace Bicicleteria.Backend.Models;

public class CarouselItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public int Range { get; set; }

}
