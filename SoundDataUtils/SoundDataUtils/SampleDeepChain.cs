using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SoundDataUtils
{
    //TODO:make this class generic
    public class SampleDeepChain
    {
        //private static readonly short[] stopSymbols = new[] { "!", ".", "?" };
        private static readonly short[] stopSymbols = { 0 };
        private static readonly short spaceSymbol = 0;
        private static readonly short startSymbol = 0;
        private Dictionary<short, Chain> chains;
        private Chain head;
        private int depth;

        /// <summary>
        /// Creates a new multi-deep Markov Chain with the depth passed in
        /// </summary>
        /// <param name="depth">The depth to store information for words.  Higher values mean more consistency but less flexibility.  Minimum value of three.</param>
        public SampleDeepChain(int depth)
        {
            if (depth < 3)
                throw new ArgumentException("We currently only support Markov Chains 3 or deeper.  Sorry :(");
            chains = new Dictionary<short, Chain>();
            head = new Chain() { val = startSymbol };
            chains.Add(startSymbol, head);
            this.depth = depth;
        }

        /// <summary>
        /// Feed in text that wil be used to create predictive text.
        /// </summary>
        /// <param name="s">The text that this Markov chain will use to generate new sentences</param>
        public void feed(short[] s)
        {
            List<short[]> sentences = getSentences(s);
            short[] valuesToAdd;

            foreach (var sentence in sentences)
            {
                for (int start = 0; start < sentence.Length - 1; start++)
                {
                    for (int end = 2; end < depth + 2 && end + start <= sentence.Length; end++)
                    {
                        valuesToAdd = new short[end];
                        for (int j = start; j < start + end; j++)
                            valuesToAdd[j - start] = sentence[j];
                        addWord(valuesToAdd);
                    }
                }
            }
        }

        /// <summary>
        /// Feed in a saved XML document of values that will be used to generate sentences.  Please note that the depth in the XML document must match the depth created by the constructor of this Markov Chain.
        /// </summary>
        /// <param name="xd">The XML document used to load this Markov Chain.</param>
        public void feed(XmlDocument xd)
        {
            XmlNode root = xd.ChildNodes[0];
            int rootDepth = Convert.ToInt32(root.Attributes["Depth"].Value.ToString());
            if (this.depth != rootDepth) //Check to make sure the depths line up
                throw new ArgumentException("The passed in XML document does not have the same depth as this MultiMarkovChain.  The depth of the Markov chain is " + this.depth.ToString() + ", the depth of the XML document is " + rootDepth.ToString() + ".  The Markov Chain depth can be modified in the constructor");

            //First add each word
            foreach (XmlNode xn in root.ChildNodes)
            {
                string text = xn.Attributes["Text"].Value.ToString();
                var val = Int16.Parse(text);
                if (!chains.ContainsKey(val))
                    chains.Add(val, new Chain() { val = val });
            }

            //Now add each next word (Trey:  I do not like this backtracking algorithm.  This could be made better.)
            List<short> nextWords;
            foreach (XmlNode xn in root.ChildNodes)
            {
                var topWord = xn.Attributes["Text"].Value.ToString();
                Queue<XmlNode> toProcess = new Queue<XmlNode>();
                foreach (XmlNode n in xn.ChildNodes)
                    toProcess.Enqueue(n);

                while (toProcess.Count != 0)
                {
                    XmlNode currentNode = toProcess.Dequeue();
                    int currentCount = Convert.ToInt32(currentNode.Attributes["Count"].Value.ToString());
                    nextWords = new List<short>();
                    nextWords.Add(Int16.Parse(topWord));
                    //nextWords.Add(currentNode.Attributes["Text"].Value.ToString());
                    XmlNode parentTrackingNode = currentNode;
                    while (parentTrackingNode.Attributes["Text"].Value.ToString() != topWord)
                    {
                        nextWords.Insert(1, Int16.Parse(parentTrackingNode.Attributes["Text"].Value));
                        parentTrackingNode = parentTrackingNode.ParentNode;
                    }
                    addWord(nextWords.ToArray(), currentCount);

                    foreach (XmlNode n in currentNode.ChildNodes)
                        toProcess.Enqueue(n);
                }
            }
        }

        private List<short[]> getSentences(short[] words)
        {
            List<short[]> sentences = new List<short[]>();
            List<short> currentSentence = new List<short>();
            currentSentence.Add(startSymbol); //start of sentence
            for (int i = 0; i < words.Length; i++)
            {
                currentSentence.Add(words[i]);
                if (stopSymbols.Contains(words[i]))
                {
                    sentences.Add(currentSentence.ToArray());
                    currentSentence = new List<short>();
                    currentSentence.Add(startSymbol);
                }
            }
            return sentences;
        }

        private void addWord(short[] words, int count = 1)
        {
            //Note:  This only adds the last word in the array. The other words should already be added by this point
            List<Chain> chainsList = new List<Chain>();
            short lastWord = words[words.Length - 1];
            for (int i = 1; i < words.Length - 1; i++)
                chainsList.Add(this.chains[words[i]]);
            if (!this.chains.ContainsKey(lastWord))
                this.chains.Add(lastWord, new Chain() { val = lastWord });
            chainsList.Add(this.chains[lastWord]);
            Chain firstChainInList = chains[words[0]];
            firstChainInList.addWords(chainsList.ToArray(), count);
        }

        /// <summary>
        /// Determines if this Markov Chain is ready to begin generating sentences
        /// </summary>
        /// <returns></returns>
        public bool readyToGenerate()
        {
            return (head.getNextWord() != null);
        }

        /// <summary>
        /// Generate a sentence based on the data passed into this Markov Chain.
        /// </summary>
        /// <returns></returns>
        public short[] generateSentence()
        {
            var sb = new ArrayBuilder<short>();
            short[] currentChains = new short[depth];
            currentChains[0] = head.getNextWord().val;
            sb.Append(currentChains[0]);
            short[] temp;
            bool doneProcessing = false;
            for (int i = 1; i < depth; i++)
            {
                //Generate the first row
                temp = new short[i];
                for (int j = 0; j < i; j++)
                    temp[j] = currentChains[j];
                var nextToHead = head.getNextWord(temp);
                if (nextToHead != null)
                    currentChains[i] = nextToHead.val;
                else
                {
                    doneProcessing = true;
                    break;
                }
                if (stopSymbols.Contains(currentChains[i]))
                {
#if !DEBUG
                                  doneProcessing = true;
      
#endif
                    sb.Append(currentChains[i]);
                    break;
                }
#if !DEBUG

                sb.Append(spaceSymbol);
#endif
                sb.Append(currentChains[i]);
            }

            int breakCounter = 0;
            while (!doneProcessing)
            {
                for (int j = 1; j < depth; j++)
                    currentChains[j - 1] = currentChains[j];
                Chain newHead = chains[currentChains[0]];
                temp = new short[depth - 2];
                for (int j = 1; j < depth - 1; j++)
                    temp[j - 1] = currentChains[j];
                var nextToNewHead = newHead.getNextWord(temp);
                if (nextToNewHead != null)
                    currentChains[depth - 1] = nextToNewHead.val;
                else
                {
                    break;
                }
                if (stopSymbols.Contains(currentChains[depth - 1]))
                {
                    sb.Append(currentChains[depth - 1]);
                    break;
                }
#if !DEBUG

                sb.Append(spaceSymbol);
#endif
                sb.Append(currentChains[depth - 1]);

                breakCounter++;
                if (breakCounter >= 50) //This is still relatively untested software.  Better safe than sorry :)
                    break;
            }

            return sb.ToArray();
        }

        private object Default = 0;

        public List<short> getNextLikelyWord(short[] previousText)
        {
            //TODO:  Do a code review of this function, it was written pretty hastily
            //TODO:  Include results that use a chain of less length that the depth.  This will allow for more results when the depth is large
            List<short> results = new List<short>();

            if (previousText.Length == 0)
            {
                //Assume start of sentence

                List<ChainProbability> nextChains = head.getPossibleNextWords(new short[0]);
                nextChains.Sort((x, y) =>
                {
                    return x.count - y.count;
                });
                foreach (ChainProbability cp in nextChains)
                    results.Add(cp.chain.val);
            }
            else
            {
                short[] initialSplit = previousText;

                short[] previousWords;
                if (initialSplit.Length > depth)
                {
                    previousWords = new short[depth];
                    for (int i = 0; i < depth; i++)
                        previousWords[i] = initialSplit[initialSplit.Length - depth + i];
                }
                else
                {
                    previousWords = new short[initialSplit.Length];
                    for (int i = 0; i < initialSplit.Length; i++)
                        previousWords[i] = initialSplit[i];
                }

                if (!chains.ContainsKey(previousWords[0]))
                    return new List<short>();

                try
                {
                    Chain headerChain = chains[previousWords[0]];
                    var sadPreviousWords = new short[previousWords.Length - 1]; //They are sad because I'm allocating extra memory for a slightly different array and there's probably a better way but I'm lazy :(
                    for (int i = 1; i < previousWords.Length; i++)
                        sadPreviousWords[i - 1] = previousWords[i];
                    List<ChainProbability> nextChains = headerChain.getPossibleNextWords(sadPreviousWords);
                    nextChains.Sort((x, y) =>
                    {
                        return x.count - y.count;
                    });
                    foreach (ChainProbability cp in nextChains)
                        results.Add(cp.chain.val);
                }
                catch (Exception excp)
                {
                    return new List<short>();
                }
            }
            return results;
        }

        /// <summary>
        /// Save the data contained in this Markov Chain to an XML document.
        /// </summary>
        /// <param name="path">The file path to save to.</param>
        public void save(string path)
        {
            XmlDocument xd = getXmlDocument();
            xd.Save(path);
        }

        /// <summary>
        /// Get the data for this Markov Chain as an XmlDocument object.
        /// </summary>
        /// <returns></returns>
        public XmlDocument getXmlDocument()
        {
            XmlDocument xd = new XmlDocument();
            XmlElement root = xd.CreateElement("Chains");
            root.SetAttribute("Depth", this.depth.ToString());
            xd.AppendChild(root);

            foreach (var key in chains.Keys)
                root.AppendChild(chains[key].getXml(xd));

            return xd;
        }

        private class Chain
        {
            internal short val;
            internal int fullCount;
            internal Dictionary<short, ChainProbability> nextNodes;

            internal Chain()
            {
                nextNodes = new Dictionary<short, ChainProbability>();
                fullCount = 0;
            }

            internal void addWords(Chain[] c, int count = 1)
            {
                if (c.Length == 0)
                    throw new ArgumentException("The array of chains passed in is of zero length.");
                if (c.Length == 1)
                {
                    this.fullCount += count;
                    if (!this.nextNodes.ContainsKey(c[0].val))
                        this.nextNodes.Add(c[0].val, new ChainProbability(c[0], count));
                    else
                        this.nextNodes[c[0].val].count += count;
                    return;
                }

                ChainProbability nextChain = nextNodes[c[0].val];
                for (int i = 1; i < c.Length - 1; i++)
                    nextChain = nextChain.getNextNode(c[i].val);
                nextChain.addWord(c[c.Length - 1], count);
            }

            internal Chain getNextWord()
            {
                int currentCount = RandomHandler.random.Next(fullCount) + 1;
                foreach (var key in nextNodes.Keys)
                {
                    currentCount -= nextNodes[key].count;
                    if (currentCount <= 0)
                        return nextNodes[key].chain;
                }
                return null;
            }

            internal Chain getNextWord(short[] words)
            {
                ChainProbability currentChain = nextNodes[words[0]];
                for (int i = 1; i < words.Length; i++)
                    currentChain = currentChain.getNextNode(words[i]);

                int currentCount = RandomHandler.random.Next(currentChain.count) + 1;
                foreach (var key in currentChain.nextNodes.Keys)
                {
                    currentCount -= currentChain.nextNodes[key].count;
                    if (currentCount <= 0)
                        return currentChain.nextNodes[key].chain;
                }
                return null;
            }

            internal List<ChainProbability> getPossibleNextWords(short[] words)
            {
                List<ChainProbability> results = new List<ChainProbability>();

                if (words.Length == 0)
                {
                    foreach (var key in nextNodes.Keys)
                        results.Add(nextNodes[key]);
                    return results;
                }

                ChainProbability currentChain = nextNodes[words[0]];
                for (int i = 1; i < words.Length; i++)
                    currentChain = currentChain.getNextNode(words[i]);

                foreach (var key in currentChain.nextNodes.Keys)
                    results.Add(currentChain.nextNodes[key]);

                return results;
            }

            internal XmlElement getXml(XmlDocument xd)
            {
                XmlElement e = xd.CreateElement("Chain");
                e.SetAttribute("Text", this.val.ToString());

                foreach (var key in nextNodes.Keys)
                    e.AppendChild(nextNodes[key].getXML(xd));

                return e;
            }
        }

        private class ChainProbability
        {
            internal Chain chain;
            internal int count;
            internal Dictionary<short, ChainProbability> nextNodes;

            internal ChainProbability(Chain c, int co)
            {
                chain = c;
                count = co;
                nextNodes = new Dictionary<short, ChainProbability>();
            }

            internal void addWord(Chain c, int count = 1)
            {
                var word = c.val;
                if (this.nextNodes.ContainsKey(word))
                    this.nextNodes[word].count += count;
                else
                    this.nextNodes.Add(word, new ChainProbability(c, count));
            }

            internal ChainProbability getNextNode(short prev)
            {
                return nextNodes[prev];
            }

            internal XmlElement getXML(XmlDocument xd)
            {
                XmlElement e = xd.CreateElement("Chain");
                e.SetAttribute("Text", chain.val.ToString());
                e.SetAttribute("Count", count.ToString());

                XmlElement c;
                foreach (var key in nextNodes.Keys)
                {
                    c = nextNodes[key].getXML(xd);
                    e.AppendChild(c);
                }

                return e;
            }
        }
    }

    public class ArrayBuilder<T>
    {
        readonly List<T> _innerList = new List<T>();
        public T[] ToArray()
        {
            return _innerList.ToArray();
        }

        public void Append(T item)
        {
            _innerList.Add(item);
        }
    }

    public static class RandomHandler
    {
        //Handles the global random object
        private static System.Random _random;
        public static System.Random random
        {
            get
            {
                if (_random == null)
                    _random = new Random();

                return _random;
            }
        }
    }
}