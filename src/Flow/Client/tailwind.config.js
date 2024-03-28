/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./**/*.{razor,html,cshtml}",
    "./node_modules/flowbite/**/*.js"
  ],
  theme: {
    screens: {
      'xs': '400px',
      'sm': '650px',
      'md': '850px',   
      'lg': '1024px',  
      'xl': '1280px',  
      '2xl': '1536px', 
    },

    extend: {},
  },
  plugins: [
    require('flowbite/plugin')
  ],
}

