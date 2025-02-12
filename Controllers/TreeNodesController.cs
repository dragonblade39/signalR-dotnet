using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Model;
using WebApplication1.Hubs;
using Microsoft.AspNetCore.SignalR;

[Route("api/[controller]")]
[ApiController]
public class TreeNodesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<TreeNodeHub> _hubContext;

    public TreeNodesController(AppDbContext context, IHubContext<TreeNodeHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
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
            await _hubContext.Clients.All.SendAsync("ReceiveTreeNode", node);
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
    [HttpGet("childrenByName/{name}")]
    public async Task<IActionResult> GetChildrenByName(string name)
    {
        // Find the parent node based on the name
        var parentNode = await _context.TreeNodes
            .FirstOrDefaultAsync(node => node.Label == name);

        if (parentNode == null)
        {
            return NotFound(new { message = "Parent node not found." });
        }

        Console.WriteLine($"Found Parent Node: {parentNode.Id} - {parentNode.Label}");

        // Get children for the parent node
        var directChildren = await _context.TreeNodes
            .Where(node => node.ParentId == parentNode.Id)
            .ToListAsync();

        if (directChildren.Count > 0)
        {
            return Ok(directChildren); // Return only direct children
        }

        // If direct children are missing, return an appropriate message
        return NotFound(new { message = "No children found for the specified node." });
    }

    // GET: api/TreeNodes/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TreeNode>> GetTreeNodeById(int id)
    {
        var node = await _context.TreeNodes
            .Include(n => n.Children) // Ensure children are loaded
            .FirstOrDefaultAsync(n => n.Id == id);

        if (node == null)
        {
            return NotFound(new { message = "Node not found." });
        }

        return Ok(node);
    }



}