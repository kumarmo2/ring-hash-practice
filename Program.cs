// See https://aka.ms/new-console-template for more information
using System.Security.Cryptography;
using System.Text;

Ring ring = new Ring();
Node n1 = new Node("1");
Node n2 = new Node("2");

ring.AddNode(n1);
ring.AddNode(n2);

foreach (var virtualNode in ring.VirtualNodes)
{
    Console.WriteLine($"virtualNode: hash: {virtualNode._hash}");
}

public static class Utils
{
    public static string GetHashHexString(string input, HashAlgorithm hasher) =>
        BitConverter.ToString(hasher.ComputeHash(Encoding.ASCII.GetBytes(input)));
}

public class Node
{
    private string _identifier;
    public string Identifier
    {
        get => _identifier;
    }

    public Node(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException(nameof(identifier));
        }
        _identifier = identifier;
    }
}

public class VirtualNode : IComparable<VirtualNode>
{
    private Node _node;
    internal int _virtualNodeIdPerNode;
    internal string _hash;
    private HashAlgorithm _hasher;

    internal VirtualNode(Node node, int virtualNodeIdPerNode, HashAlgorithm hasher)
    {
        if (virtualNodeIdPerNode < 0)
        {
            throw new ArgumentException($"invalid {nameof(virtualNodeIdPerNode)}");
        }

        if (hasher is null)
        {
            throw new ArgumentNullException(nameof(hasher));
        }
        _hasher = hasher;
        _node = node ?? throw new ArgumentNullException(nameof(node));
        var combinedIdentifier = $"{node.Identifier}-{virtualNodeIdPerNode}";
        _hash = Utils.GetHashHexString(combinedIdentifier, _hasher);
    }

    public int CompareTo(VirtualNode? other)
    {
        return _hash.CompareTo(other?._hash);
    }
}

/*
 * for the first phase, lets not worry about the dynamic changes in the ring.
 * i.e: servers getting added/removed once the ring has been setup and in use.
 * */
public class Ring : IDisposable
{
    private List<VirtualNode> _ring;
    private int _nodes;

    // TODO: make it configurable later on.
    private int _numOfVirtualNodesPerNode = 5;
    private MD5 _hasher;

    public Ring()
    {
        _ring = new List<VirtualNode>();
        _hasher = MD5.Create();
    }

    public List<VirtualNode> VirtualNodes
    {
        get => _ring;
    }

    public Node AssignNode(string input)
    {
        var hash = Utils.GetHashHexString(input, _hasher);
        // TODO:
        // do the binary search on the input and the List<VirtualNode> and return
        // the node of the virutal node
        throw new NotImplementedException();
    }

    public void AddNode(Node node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }
        for (var i = 0; i < _numOfVirtualNodesPerNode; i++)
        {
            var virtualNode = new VirtualNode(node, i, _hasher);
            _ring.Add(virtualNode);
        }

        _ring.Sort();

        _nodes++;

        /*
         * if the node has been already added to the ring, throw error.
         * create virtual nodes of this node.
         * add the virtual nodes to _inner
         * */
    }

    public void Dispose()
    {
        // TODO: provide correct implementation of this.
        if (_hasher is not null)
        {
            _hasher.Dispose();
        }
    }
}
