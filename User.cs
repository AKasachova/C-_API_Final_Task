public class User : IEquatable<User>
{
    public string Name { get; set; }
    public int? Age { get; set; }
    public string Sex { get; set; }
    public string? ZipCode { get; set; }

    public override bool Equals(object obj)
    {
        return this.Equals(obj as User);
    }

    public bool Equals(User other)
    {
        if (other == null)
            return false;
        return Name == other.Name && Age == other.Age && Sex == other.Sex && ZipCode == other.ZipCode;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Name.GetHashCode();
            hash = hash * 23 + (Age != null ? Age.GetHashCode() : 0);
            hash = hash * 23 + Sex.GetHashCode();
            hash = hash * 23 + (ZipCode != null ? ZipCode.GetHashCode() : 0);
            return hash;
        }
    }

}
