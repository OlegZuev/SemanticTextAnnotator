using System.Collections.Generic;
using Python.Runtime;

namespace SemanticTextAnnotator {
    public class SemanticTextAnnotator {
        public static List<NamedEntity> ParseEntities(string text) {
            var entitySpans = new List<NamedEntity>();
            using (Py.GIL()) {
                dynamic natasha = Py.Import("natasha");
                dynamic builtins = Py.Import("builtins");
                //Console.WriteLine(np.cos(np.pi * 2));
                dynamic segmenter = natasha.Segmenter();
                dynamic morph_vocab = natasha.MorphVocab();
                dynamic emb = natasha.NewsEmbedding();
                dynamic morph_tagger = natasha.NewsMorphTagger(emb);
                dynamic syntax_parser = natasha.NewsSyntaxParser(emb);
                dynamic ner_tagger = natasha.NewsNERTagger(emb);
                dynamic names_extractor = natasha.NamesExtractor(morph_vocab);

                dynamic doc = natasha.Doc(text);
                doc.segment(segmenter);
                doc.tag_morph(morph_tagger);
                doc.parse_syntax(syntax_parser);
                doc.tag_ner(ner_tagger);
                int length = builtins.len(doc.spans);
                for (int i = 0; i < length; i++)
                {
                    doc.spans[i].normalize(morph_vocab);
                    entitySpans.Add(new NamedEntity
                    {
                        start = doc.spans[i].start,
                        stop = doc.spans[i].stop,
                        type = doc.spans[i].type,
                        text = doc.spans[i].text,
                        normal = doc.spans[i].normal,
                    });
                }
            }

            return entitySpans;
        }
    }
}