using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Anagrams
{
    public partial class Form1 : Form
    {
        List<bag_and_anagrams> dictionary;
        DateTime start_time;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Bag.test();
        }

        // this is a filter for entires in the original word list.  It rejects words that have no vowels, and those that are too short.
        private bool acceptable(string s)
        {
            if (s.Length < 2)
            {
                if (s == "i" || s == "a")
                    return true;
                return false;
            }
            char[] vowels = { 'a', 'e', 'i', 'o', 'u', 'y' };
            if (s.IndexOfAny(vowels, 0) > -1)
                return true;
            return false;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            System.IO.Stream wordlist_stream;
            System.Reflection.Assembly thisExe;
            thisExe = System.Reflection.Assembly.GetExecutingAssembly();
            wordlist_stream =
                thisExe.GetManifestResourceStream("Anagrams.words");
            System.Diagnostics.Trace.Assert(wordlist_stream != null,
                "Uh oh, can't find word list inside myself!");
            toolStripStatusLabel1.Text = "Compiling dictionary ...";
            ProgressBar.Value = 0;
            ProgressBar.Maximum = (int)wordlist_stream.Length;
            listView1_Resize(sender, e);
            using (StreamReader sr = new StreamReader(wordlist_stream))
            {
                String line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                int linesRead = 0;
                Hashtable stringlists_by_bag = new Hashtable();
                while ((line = sr.ReadLine()) != null)
                {
                    // TODO -- filter out nonletters.  Thus "god's"
                    // should become "gods".  And since both of those
                    // are likely to appear, we need to ensure that we
                    // only store one.
                    line = line.ToLower();
                    if (!acceptable(line))
                        continue;
                    Bag aBag = new Bag(line);
                    if (!stringlists_by_bag.ContainsKey(aBag))
                    {
                        strings l = new strings();
                        l.Add(line);
                        stringlists_by_bag.Add(aBag, l);
                    }
                    else
                    {
                        strings l = (strings)stringlists_by_bag[aBag];
                        if (!l.Contains(line))
                            l.Add(line);
                    }
                    linesRead++;
                    ProgressBar.Increment(line.Length + 1); // the +1 is for the line ending character, I'd guess.

                    Application.DoEvents();
                }

                // Now convert the hash table, which isn't useful for
                // actually generating anagrams, into a list, which is.

                dictionary = new List<bag_and_anagrams>();
                foreach (DictionaryEntry de in stringlists_by_bag)
                {
                    dictionary.Add(new bag_and_anagrams((Bag)de.Key, (strings)de.Value));
                }

                // Now just for amusement, sort the list so that the biggest bags 
                // come first.  This might make more interesting anagrams appear first.
                bag_and_anagrams[] sort_me = new bag_and_anagrams[dictionary.Count];
                dictionary.CopyTo(sort_me);
                Array.Sort(sort_me);
                dictionary.Clear();
                dictionary.InsertRange(0, sort_me);
            }
            toolStripStatusLabel1.Text = "Compiling dictionary ... done.";
            listView1.Enabled = true;
            input.Enabled = true;
            input.Focus();
        }

        private void anagrams_Click(object sender, EventArgs e)
        {
            input.Enabled = false;
            Bag input_bag = new Bag(input.Text);
            listView1.Items.Clear();
            fileToolStripMenuItem.Enabled = false;
            start_time = DateTime.Now;
            elapsed_time.Text = "00:00:00";
            timer1.Enabled = true;
            ProgressBar.Value = 0;
            Anagrams.anagrams(input_bag, dictionary, 0,

                // bottom of main loop
                delegate()
                {
                    ProgressBar.PerformStep();
                    Application.DoEvents();
                },

                // done pruning
                delegate(uint recursion_level, List<bag_and_anagrams> pruned_dict)
                {
                    if (recursion_level == 0)
                    {
                        ProgressBar.Maximum = pruned_dict.Count;
                        Application.DoEvents();
                    }
                },

                // found a top-level anagram
                delegate(strings words)
                {
                    string display_me = "";
                    foreach (string s in words)
                    {
                        if (display_me.Length > 0)
                            display_me += " ";
                        display_me += s;
                    }

                    listView1.Items.Add(display_me);
                    listView1.EnsureVisible(listView1.Items.Count - 1);
                    toolStripStatusLabel1.Text = listView1.Items.Count.ToString() + " anagrams so far";
                    if (listView1.Items.Count % 1000 == 0)
                    {
                        Application.DoEvents();
                    }

                });
            timer1.Enabled = false;
            toolStripStatusLabel1.Text = String.Format("Done.  {0} anagrams",
                listView1.Items.Count);
            if (listView1.Items.Count > 0)
                listView1.EnsureVisible(0);
            input.Enabled = true;
            input.Focus();
            // the leading spaces work around a bug in the control: I
            // want the text centered, but that doesn't work.
            // Another workaround is for me to handle the
            // DrawColumnHeader event myself, but I'm too lazy to do that.
            listView1.Columns[0].Text = "                   Click to sort";
            fileToolStripMenuItem.Enabled = true;
        }

        private void input_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                anagrams_Click(sender, e);

            // This smells.  I want to trap Control-A, so that I can
            // select all the text in the input box (control-A does
            // just that in other contexts, but not here, for some
            // reason).  But I don't know the politically-correct way
            // to spell Control-A, so I just use 1.
            if (e.KeyChar == (char)1)
                input.SelectAll();
        }

        private void listView1_Resize(object sender, EventArgs e)
        {
            // trial and error shows that we must make the column
            // header four pixels narrower than the containing
            // listview in order to avoid a scrollbar.
            listView1.Columns[0].Width = listView1.Width - 4;

            // if the listview is big enough to show all the items, then make sure
            // the first item is at the top.  This works around behavior (which I assume is 
            // a bug in C# or .NET or something) whereby 
            // some blank lines appear before the first item

            if (listView1.Items.Count > 0
                &&
                listView1.TopItem != null
                &&
                listView1.TopItem.Index == 0)
                listView1.EnsureVisible(0);

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Clipboard.Clear();

            string selected_text = "";
            ListView me = (ListView)sender;
            foreach (ListViewItem it in me.SelectedItems)
            {
                if (selected_text.Length > 0)
                    selected_text += Environment.NewLine;
                selected_text += it.Text;
            }
            // Under some circumstances -- probably a bug in my code somewhere --
            // we can get blank lines in the listview.  And if you click on one, since it
            // has no text, selected_text will be blank; _and_, apparantly, calling
            // Clipboard.set_text with an empty string provokes an access violation ...
            // so avoid that AV.
            if (selected_text.Length > 0)
                Clipboard.SetText(selected_text);
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            listView1.Sorting = SortOrder.Ascending;
            listView1.Sorting = SortOrder.None;
            listView1.Columns[0].Text = "";
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = input.Text;
            saveFileDialog1.InitialDirectory = Application.LocalUserAppDataPath;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.OpenFile()))
                {
                    sw.WriteLine("{0} anagrams of '{1}'",
                        listView1.Items.Count, input.Text);
                    sw.WriteLine("-----------------------");
                    foreach (ListViewItem it in listView1.Items)
                    {
                        sw.WriteLine(it.Text);
                    }
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(String.Format("version {0}", Application.ProductVersion),
                Application.ProductName);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            elapsed_time.Text = DateTime.Now.Subtract(start_time).ToString();
        }
    }
}
