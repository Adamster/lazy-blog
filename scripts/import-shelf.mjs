#!/usr/bin/env node

// Owner-authenticated one-off import through the same atomic create/update API as the UI.
// The server decodes, normalizes, hashes and uploads each cover. Legacy `read` is ignored.
import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import { basename, resolve, sep } from "node:path";

const [manifestPath, assetSourceRoot, apiBase = "http://localhost:5000", cliToken] = process.argv.slice(2);
const token = process.env.SHELF_ADMIN_TOKEN || cliToken;
if (!manifestPath || !assetSourceRoot || !token) {
  console.error("Usage: node scripts/import-shelf.mjs <manifest.json> <asset-source-root> [api-base] [jwt-or-SHELF_ADMIN_TOKEN]");
  process.exit(1);
}
if (cliToken) console.warn("Warning: passing the token as a CLI argument exposes it in shell history and process listings; prefer SHELF_ADMIN_TOKEN.");
const manifest = JSON.parse(await readFile(manifestPath, "utf8"));
if (manifest.schemaVersion !== 1 || !Array.isArray(manifest.genres) || !Array.isArray(manifest.books) || !Array.isArray(manifest.assets)) throw new Error("Malformed manifest");
const root = resolve(assetSourceRoot), api = apiBase.replace(/\/$/, "");
const auth = { authorization: `Bearer ${token}` };
const request = async (path, options={}) => { const r=await fetch(api+path,{...options,headers:{...auth,...options.headers}});if(!r.ok)throw new Error(`${options.method||"GET"} ${path}: ${r.status} ${await r.text()}`);return r; };

let genres = await (await request("/api/admin/genres")).json();
for (const [sortOrder, genre] of manifest.genres.entries()) if (!genres.some(x=>x.slug===genre.id))
  await request("/api/admin/genres",{method:"POST",headers:{"content-type":"application/json"},body:JSON.stringify({slug:genre.id,name:genre.label,sortOrder})});
genres = await (await request("/api/admin/genres")).json();
const genreIds = new Map(genres.map(x=>[x.slug,x.id]));
const normalize=value=>value.trim().replace(/\s+/g," ").toLocaleUpperCase("ru");
const authorIds=new Map();
for(const name of [...new Set(manifest.books.map(x=>x.author))]){const created=await(await request("/api/admin/authors",{method:"POST",headers:{"content-type":"application/json"},body:JSON.stringify({name})})).json();authorIds.set(normalize(name),created.id);}
const assets = new Map(manifest.assets.map(x=>[x.targetKey,x]));
const existing=[];for(let offset=0;;offset+=100){const page=await(await request(`/api/admin/books?offset=${offset}&limit=100`)).json();existing.push(...page.items);if(existing.length>=page.total)break;}
const existingBySource=new Map(existing.filter(x=>x.sourceWorkId).map(x=>[x.sourceWorkId,x]));
const usedSlugs=new Set(existing.map(x=>x.slug));
const slugify=value=>{const base=value.toLocaleLowerCase("ru").normalize("NFKD").replace(/[^\p{L}\p{N}]+/gu,"-").replace(/^-|-$/g,"").slice(0,150);let s=base;for(let n=2;usedSlugs.has(s);n++)s=`${base}-${n}`;usedSlugs.add(s);return s;};

const failures = [];
for (const [index, book] of manifest.books.entries()) {
  try {
    const asset=assets.get(book.coverKey);if(!asset)throw new Error(`Missing asset ${book.coverKey}`);
    const path=resolve(root,asset.sourcePath);if(path!==root&&!path.startsWith(root+sep))throw new Error(`Asset escapes root: ${asset.sourcePath}`);
    const bytes=await readFile(path);if(createHash("sha256").update(bytes).digest("hex")!==asset.sha256)throw new Error(`Checksum mismatch: ${asset.sourcePath}`);
    const prior=existingBySource.get(book.sourceWorkId);const form=new FormData();
    const fields={genreId:genreIds.get(book.genreId),authorId:authorIds.get(normalize(book.author)),slug:prior?.slug||slugify(`${book.author}-${book.title}`),title:book.title,series:book.seriesName,rating:String(book.rating??0),ratingCount:String(book.ratingCount??0),ratingSource:book.source==="fantlab"?"FantLab":book.source,sourceWorkId:book.sourceWorkId,sourceUrl:book.sourceUrl};
    for(const [k,v] of Object.entries(fields))if(v!==null&&v!==undefined)form.append(k,String(v));for(const title of book.seriesTitles??[])form.append("seriesTitles",title);
    form.append("cover",new Blob([bytes],{type:asset.mimeType}),basename(asset.sourcePath));
    await request(prior?`/api/admin/books/${prior.id}`:"/api/admin/books",{method:prior?"PUT":"POST",body:form});
    console.log(`[${index+1}/${manifest.books.length}] ${book.title}`);
  } catch (error) {
    failures.push({ index, title: book.title, error });
    console.error(`[${index+1}/${manifest.books.length}] FAILED ${book.title}: ${error.message}`);
  }
}
if (failures.length > 0) {
  console.error(`\n${failures.length} of ${manifest.books.length} book(s) failed to import:`);
  for (const failure of failures) console.error(`  [${failure.index+1}] ${failure.title}: ${failure.error.message}`);
  process.exitCode = 1;
}
