using System.Collections.Generic;

namespace GymApp.Models
{
    public class TrainerServicesViewModel
    {
        public int TrainerId { get; set; }
        public string TrainerName { get; set; } = null!;

        // Checkbox listesi için
        public List<ServiceCheckboxItem> Services { get; set; } = new List<ServiceCheckboxItem>();

        // Formdan gelen seçili hizmet Id’leri
        public List<int> SelectedServiceIds { get; set; } = new List<int>();
    }

    public class ServiceCheckboxItem
    {
        public int ServiceId { get; set; }
        public string Name { get; set; } = null!;
        public bool IsSelected { get; set; }
    }
}
