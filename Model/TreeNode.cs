using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Model
{
    public class TreeNode
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Label { get; set; }

        public int IsActive { get; set; } //0=grey 1=red 2=green

        public int? ParentId { get; set; }
        public List<TreeNode>? Children { get; set; }



    }
}
