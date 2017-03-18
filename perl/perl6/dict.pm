unit module dict;

our @dict;

my $dict_file_name = #"../../words.utf8";
                     "words";

sub acceptable (Str $word) returns Bool {
  if $word ~~ m{<-alpha>} {
    return Bool::False ;
  }

  if ($word ~~ "i") {
    return Bool::True ;
  }

  if ($word ~~ "a") {
    return Bool::True ;
  }

  if ($word.chars < 2) {
    return Bool::False ;
  }

  if ($word ~~ m{<[aeiouy]>}) {
    return Bool::True ;
  }

  return Bool::False;
}

sub snarf_wordlist {
  my $dict = open($dict_file_name, :r)
    or die "Can't open $dict_file_name for reading 'cuz $!; stopped";

  note "Reading $dict_file_name ...";

  my %words_by_bag is default(SetHash);

  for ($dict.words) {
      my $word = lc($_);
      if acceptable($word) {
          my $bag = $word.ords.Bag;
          %words_by_bag{$bag}{$word} = True;
      }
  };
  note " done";
  close ($dict) or die "Closing $dict: $!";

  note "Now rejiggering that data into a nice list ...";

  for %words_by_bag.pairs -> $bag, $words {
      @dict.push($bag, $words);
  }
  note " done";
}

snarf_wordlist();
say @dict.perl;
