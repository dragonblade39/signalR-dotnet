using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Model;

[Route("api/[controller]")]
[ApiController]
public class TreeNodesController : ControllerBase
{
    private readonly AppDbContext _context;

    public TreeNodesController(AppDbContext context)
    {
        _context = context;
    }

    // POST: api/TreeNodes
    [HttpPost]
    public async Task<IActionResult> PostTreeNodes([FromBody] List<TreeNode> treeNodes)
    {
        if (treeNodes == null || treeNodes.Count == 0)
        {
            return BadRequest("Tree nodes are required.");
        }

        foreach (var node in treeNodes)
        {
            // Insert each node and its children
            await InsertNode(node, null);
        }

        await _context.SaveChangesAsync(); // Save once after all nodes are queued
        return CreatedAtAction(nameof(GetTreeNodes), new { }, treeNodes);
    }

    private async Task InsertNode(TreeNode node, int? parentId)
    {
        node.ParentId = parentId;
        _context.TreeNodes.Add(node);

        // Instead of saving changes here, simply add the node to the context

        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                await InsertNode(child, node.Id); // Recursively add children
            }
        }
    }

    // GET: api/TreeNodes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TreeNode>>> GetTreeNodes()
    {
        var treeNodes = await _context.TreeNodes.ToListAsync();
        return Ok(treeNodes); // Return OK status with the retrieved nodes
    }

    // GET: api/TreeNodes/parents
    [HttpGet("parents")]
    public async Task<ActionResult<IEnumerable<TreeNode>>> GetParentNodes()
    {
        var parents = await _context.TreeNodes
            .Where(node => node.ParentId == null)
            .ToListAsync();

        return Ok(parents);
    }

    // GET: api/TreeNodes/children/{id}
    [HttpGet("children/{id}")]
    public async Task<ActionResult<IEnumerable<TreeNode>>> GetChildren(int id)
    {
        var children = await _context.TreeNodes
            .Where(node => node.ParentId == id)
            .ToListAsync();

        if (children == null || children.Count == 0)
        {
            return NotFound("No children found for the specified parent.");
        }

        return Ok(children);
    }

    // GET: api/TreeNodes/childrenByName/{name}
    [HttpGet("childrenByName/{name}")]
    public async Task<ActionResult<IEnumerable<TreeNode>>> GetChildrenByName(string name)
    {
        var parentNode = await _context.TreeNodes
            .FirstOrDefaultAsync(node => node.Label == name);

        if (parentNode == null)
        {
            return NotFound("Parent node not found.");
        }

        var children = await _context.TreeNodes
            .Where(node => node.ParentId == parentNode.Id)
            .ToListAsync();

        return Ok(children);
    }
}