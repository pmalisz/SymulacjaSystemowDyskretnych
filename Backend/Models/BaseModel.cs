namespace Backend.Models
{
    public abstract class BaseModel
    {
        public string Id { get; set; }

        public override bool Equals(object obj) => obj != null && (obj as BaseModel).Id == Id;

        public override int GetHashCode() => Id.GetHashCode();
    }
}
