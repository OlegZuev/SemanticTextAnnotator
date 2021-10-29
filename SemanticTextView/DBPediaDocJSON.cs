namespace SemanticTextAnnotator {
    public class DBPediaDocJSON {
        public string[] score { get; set; }

        public string[] refCount { get; set; }

        public string[] resource { get; set; }

        public string[] redirectlabel { get; set; }

        public string[] comment { get; set; }

        public string[] label { get; set; }

        public string[] category { get; set; }

        public string[] typeName { get; set; }

        public string[] type { get; set; }

        public override string ToString() {
            return label.Length > 0 ? label[0] : "Empty";
        }
    }
}