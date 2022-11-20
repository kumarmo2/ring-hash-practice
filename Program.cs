// See https://aka.ms/new-console-template for more information
using System.Security.Cryptography;
using System.Text;

Ring ring = new Ring();
Node n1 = new Node("1");
Node n2 = new Node("4");

ring.AddNode(n1);
ring.AddNode(n2);

foreach (var virtualNode in ring.VirtualNodes)
{
    Console.WriteLine($"Node, identifier{virtualNode._node.Identifier}, virtualNode: hash: {virtualNode._hash} ");
}

var target = "-1";
var hasher = MD5.Create();
Console.WriteLine($"target: {target}, hash: {Utils.GetHashHexString(target, hasher)}");

var assinged = ring.AssignNode(target);

Console.WriteLine($"AssignNode, identifier: {assinged.Identifier}");

public static class Utils
{
    public static string GetHashHexString(string input, HashAlgorithm hasher) =>
        BitConverter.ToString(hasher.ComputeHash(Encoding.ASCII.GetBytes(input)));



    // Given a sorted array and a target, find the index of target in the array. If target is not
    // found, find the next larger number than target and return its index.
    // if target is largest than largest in items, it must return the smallest in items.
    // if target is smaller than smallest in iterms, then it must return the smallest.
    public static VirtualNode? CyclicBinarySearch(List<VirtualNode> items, string targetHash)
    {
        if (items is null || items.Count == 0)
        {
            return null;
        }

        var n = items.Count;
        var left = 0;
        var right = n - 1;
        var mid = (left + right) / 2;


        while (true)
        {
            if (left > right)
            {
                break;
            }
            mid = (left + right) / 2;
            var mid_value = items[mid]._hash;

            var comparison = targetHash.CompareTo(mid_value);
            if (comparison == 0)
            {
                return items[mid];
            }

            if (comparison > 0)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        if (mid == 0)
        {
            return items[0];
        }
        else if (left > n - 1)
        {
            return items[0];
        }
        else
        {
            if (items[right]._hash.CompareTo(targetHash) > 0)
            {
                return items[right];
            }
            return items[left];
        }
    }
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
    internal Node _node;
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

    public Node? AssignNode(string input)
    {
        if (_ring.Count == 0)
        {
            throw new Exception("add nodes to the ring");
        }
        var hash = Utils.GetHashHexString(input, _hasher);

        var virtualNode = Utils.CyclicBinarySearch(_ring, hash);
        if (virtualNode is null)
        {
            /*Impossible to happen*/
            return null;
        }
        return virtualNode._node;

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
