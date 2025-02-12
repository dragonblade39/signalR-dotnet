using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Model
{
    public class TreeNode
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Label { get; set; }

        public bool IsActive { get; set; }

        public int? ParentId { get; set; }
        public List<TreeNode>? Children { get; set; }



    }
}
