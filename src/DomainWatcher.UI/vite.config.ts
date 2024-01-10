import { defineConfig } from 'vite'
import solid from 'vite-plugin-solid'
import { viteSingleFile } from "vite-plugin-singlefile"

export default defineConfig({
  plugins: [solid(), viteSingleFile()],
})
