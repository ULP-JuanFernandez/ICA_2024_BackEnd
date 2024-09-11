namespace ICA.Models
{
    
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // Agregar esta propiedad si es necesario
        public string? Message { get; set; }
    }
}
