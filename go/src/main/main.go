package main

import (
	"anagrams"
	"fmt"
	"log"
)

func main() {
	dictslice, error := anagrams.SnarfDict("/usr/share/dict/words")

	if error != nil {
		log.Fatal(error)
	}

	fmt.Printf("Number of somethings in the dictionary: %v\n", len (dictslice))

	const word = "goon"
	bag := anagrams.WordToBag (word)

	pruned_dict := anagrams.Filter(dictslice, bag)

	fmt.Printf("Anagrams of '%s': %v",
		word,
		anagrams.Anagrams (pruned_dict, bag))
}
