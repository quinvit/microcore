namespace Hyperscale.Microcore.SharedLogic.LoadBalancer
{
    public class Node
    {
        public Node(string hostName, int? port = null)
        {
            Hostname = hostName;
            Port = port;
        }

        public string Hostname { get; }
        public int? Port { get; }

        public override string ToString()
        {
            return Port.HasValue ? $"{Hostname}:{Port}" : Hostname;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Node other))
                return false;

            return other.Hostname == Hostname && other.Port == Port;
        }


        public override int GetHashCode()
        {
            unchecked
            {
                return ((Hostname?.GetHashCode() ?? 0) * 397) ^ (Port?.GetHashCode() ?? 1);
            }
        }
    }
}
