using System;

namespace CertificateApp.Models
{
    public class CertificateRecord
    {
        public int Id { get; set; }
        
        // الشاهد الأول
        public string Witness1Name { get; set; } = string.Empty;
        public string Witness1Birth { get; set; } = string.Empty;
        public string Witness1Address { get; set; } = string.Empty;
        public string Witness1Card { get; set; } = string.Empty;
        public string Witness1CardDate { get; set; } = string.Empty;
        
        // الشاهد الثاني
        public string Witness2Name { get; set; } = string.Empty;
        public string Witness2Birth { get; set; } = string.Empty;
        public string Witness2Address { get; set; } = string.Empty;
        public string Witness2Card { get; set; } = string.Empty;
        public string Witness2CardDate { get; set; } = string.Empty;

        // المعني (المسمى)
        public string TargetName { get; set; } = string.Empty;
        public string TargetBirth { get; set; } = string.Empty;
        public string TargetCard { get; set; } = string.Empty;
        public string TargetCardDate { get; set; } = string.Empty;
        public string LatinName { get; set; } = string.Empty;
        
        // تاريخ الإصدار
        public string IssueDate { get; set; } = string.Empty;
        
        // آخر تعديل
        public string LastModified { get; set; } = string.Empty;
    }
}
