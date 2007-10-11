thisdir = Documentation
include ../build/rules.make

DOC_MAN1=$(patsubst %.txt,%.1,$(wildcard *.txt))
DOC_HTML=$(patsubst %.txt,%.html,$(wildcard *.txt))

mandir?=$(prefix)/share/man
man1dir=$(mandir)/man1

INSTALL?=install

all-local: $(DOC_MAN1) $(DOC_HTML)

clean-local:
	rm -f *.xml *.html *.1

%.html : %.txt
	rm -f $@+ $@
	asciidoc -b xhtml11 -d manpage -f asciidoc.conf -o - $< >$@+
	mv $@+ $@

%.xml : %.txt
	asciidoc -b docbook -d manpage -f asciidoc.conf $<

%.1 : %.xml
	xmlto -m callouts.xsl man $<

install-local: $(DOC_MAN1) $(DOC_HTML)
	$(INSTALL) -d -m755 $(DESTDIR)$(man1dir)
	$(INSTALL) -m644 $(DOC_MAN1) $(DESTDIR)$(man1dir)

uninstall-local:
	rm -f $(DESTDIR)$(man1dir)/$(DOC_MAN1)

DISTFILES= asciidoc.conf callouts.xsl Makefile tf.txt

dist-local: dist-default

test-local:

run-test-local:
